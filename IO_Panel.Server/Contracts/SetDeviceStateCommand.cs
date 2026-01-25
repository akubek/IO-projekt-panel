using System;

namespace IO_projekt_symulator.Server.Contracts
{
    /// <summary>
    /// Messaging contract published by the panel and consumed by the simulator to set a device state.
    /// This contract MUST match the simulator's expected message shape.
    /// </summary>
    public class SetDeviceStateCommand
    {
        public Guid DeviceId { get; set; }
        public double Value { get; set; }
        public string? Unit { get; set; }
    }
}