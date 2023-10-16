namespace Trianon.Bus.Spike
{
    public record SpikeMessage()
    {
        public Guid Id { get; set; }
        public string Value { get; init; } = string.Empty;
    }
}