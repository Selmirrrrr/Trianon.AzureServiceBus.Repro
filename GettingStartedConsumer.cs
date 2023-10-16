using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace Trianon.Bus.Spike
{
    public class GettingStartedConsumer(IMemoryCache cache, ILogger<GettingStartedConsumer> logger) : IConsumer<SpikeMessage>
    {
        readonly ILogger<GettingStartedConsumer> _logger = logger;
        readonly IMemoryCache _cache = cache;

        public Task Consume(ConsumeContext<SpikeMessage> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message.Value);
            _cache.Set<SpikeMessage>(context.Message.Id, context.Message, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }
    }
}