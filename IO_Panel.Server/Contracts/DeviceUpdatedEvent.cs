namespace IO_projekt_symulator.Server.Contracts
{
    public class DeviceUpdatedEvent
    {
        public Guid DeviceId { get; set; }
        public double? Value { get; set; }
        public string? Unit { get; set; }
        public bool? Malfunctioning { get; set; }
    }
}