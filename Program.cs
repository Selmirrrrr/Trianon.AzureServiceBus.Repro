using System.Net;
using System.Reflection;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MassTransit;
using MassTransit.AzureServiceBusTransport.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Caching.Memory;
using Trianon.Bus.Spike;

var builder = WebApplication.CreateBuilder(args);

var host = builder.Configuration["AzureServiceBusSettingsManagedIdentity:Host"];
var connectionString = builder.Configuration["AzureServiceBusSettingsManagedIdentity:ConnectionString"];

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

var proxyDefault = new WebProxy("http://dirproxy.mobi.ch:80", true)
{
    Credentials = CredentialCache.DefaultCredentials
};

builder.Services.AddMassTransit(x =>
{
    var entryAssembly = Assembly.GetEntryAssembly();
    
    var handler = new HttpClientHandler
    {
        Proxy = proxyDefault,
        DefaultProxyCredentials = CredentialCache.DefaultCredentials,
        UseDefaultCredentials = true
    };
    x.AddConsumers(entryAssembly);
    // x.UsingAzureServiceBus((context, config) =>
    // {
    //     var asbClient = new ServiceBusClient(connectionString, new ServiceBusClientOptions
    //     {
    //         RetryOptions = new ServiceBusRetryOptions
    //         {
    //             MaxRetries = 1
    //         },
    //         TransportType = ServiceBusTransportType.AmqpWebSockets,
    //         WebProxy = proxyDefault
    //     });
    //
    //     var asbMgmtClient = new ServiceBusAdministrationClient(connectionString, new ServiceBusAdministrationClientOptions
    //     {
    //         Transport = new HttpClientTransport(handler)
    //     });
    //
    //     config.Host(new Uri(host), asbClient, asbMgmtClient);
    // });
    x.UsingAzureServiceBus((context, cfg) =>
    {
        var creds = new DefaultAzureCredential();
        var settings = new HostSettings
        {
            ServiceUri = new Uri("sb://trnasbqa.servicebus.windows.net"),
            TokenCredential = creds,
            ServiceBusClient = new ServiceBusClient("trnasbqa.servicebus.windows.net", creds, new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 1
                },
                TransportType = ServiceBusTransportType.AmqpWebSockets,
                WebProxy = proxyDefault
            }),
            ServiceBusAdministrationClient = new ServiceBusAdministrationClient("trnasbqa.servicebus.windows.net", creds, new ServiceBusAdministrationClientOptions
            {
                Transport = new HttpClientTransport(handler)
            })
        };
        cfg.Host(settings);
    });
});
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/bus", ([FromServices] IMemoryCache cache, [FromQuery] Guid id) =>
    {
        cache.TryGetValue<SpikeEvent>(id, out var message);

        return message;
    })
    .WithName("Consume")
    .WithOpenApi();

app.MapPost("bus", async (IBus bus, CancellationToken ct) =>
    {
        var message = new SpikeEvent()
        {
            Id = Guid.NewGuid(),
            Value = $"The time is {DateTimeOffset.Now}"
        };
        await bus.Publish(message, ct);
        return message;
    })
    .WithName("Publish")
    .WithOpenApi();

app.Run();