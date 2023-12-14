namespace Trianon.Bus.Spike
{
    public record SpikeEvent()
    {
        public Guid Id { get; set; }
        public string Value { get; init; } = string.Empty;
    }
}