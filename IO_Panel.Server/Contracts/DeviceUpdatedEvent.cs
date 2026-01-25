namespace IO_projekt_symulator.Server.Contracts
{
    /// <summary>
    /// Messaging contract published by the simulator when a device state changes.
    /// Consumed by the panel to update UI clients (SignalR) and optionally persist history.
    /// This contract MUST match the simulator's expected message shape.
    /// </summary>
    public class DeviceUpdatedEvent
    {
        /// <summary>
        /// Device identifier (GUID) matching the configured device id in the panel.
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Updated numeric value, if provided by the simulator.
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// Unit of the value (e.g., "bool", "%", "C"). May be omitted by the simulator.
        /// </summary>
        public string? Unit { get; set; }

        /// <summary>
        /// Indicates whether the simulator reports the device as malfunctioning.
        /// </summary>
        public bool? Malfunctioning { get; set; }
    }
}