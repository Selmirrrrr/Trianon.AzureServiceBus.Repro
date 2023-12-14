using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace Trianon.Bus.Spike
{
    public class SpikeConsumer : IConsumer<SpikeEvent>
    {
        public SpikeConsumer(IMemoryCache cache, ILogger<SpikeConsumer> logger)
        {
            _logger = logger;
            _cache = cache;
        }

        readonly ILogger<SpikeConsumer> _logger;
        readonly IMemoryCache _cache;

        public Task Consume(ConsumeContext<SpikeEvent> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message.Value);
            _cache.Set<SpikeEvent>(context.Message.Id, context.Message, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }
    }
}