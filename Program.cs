using System.Reflection;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Trianon.Bus.Spike;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddMassTransit(x =>
{
    var entryAssembly = Assembly.GetEntryAssembly();
    x.AddConsumers(entryAssembly);
    x.UsingAzureServiceBus((context,cfg) =>                
    {
        cfg.Host("lol");
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/bus", ([FromServices] IMemoryCache cache, [FromQuery] Guid id) =>
{
    cache.TryGetValue<SpikeMessage>(id, out var message);

    return message;
})
.WithName("Consume")
.WithOpenApi();

app.MapPost("bus", async (IBus bus, CancellationToken ct, [FromQuery] string? city = "Lausanne") => 
{
    var message = new SpikeMessage 
    {
        Id = Guid.NewGuid(),
        Value = $"The time in {city} is {DateTimeOffset.Now}" 
    };
    await bus.Publish(message, ct);
    return message; 
})
.WithName("Publish")
.WithOpenApi();;

app.Run();