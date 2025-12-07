using System;

namespace IO_Panel.Server.Contracts
{
    // This contract MUST match the one used by the Simulator.
    public class SetDeviceStateCommand
    {
        public Guid DeviceId { get; set; }
        public double Value { get; set; }
        public string? Unit { get; set; }
    }
}