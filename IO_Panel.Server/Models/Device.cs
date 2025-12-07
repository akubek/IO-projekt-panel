namespace IO_Panel.Server.Models
{
    // Domain / view model used by controllers, UI and persistence layers
    public class Device
    {
        // local identifier (may be from API or generated locally)
        public string Id { get; set; } = default!;

        public string Name { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string Location { get; set; } = default!;
        public string Description { get; set; } = default!;

        // keep nested types similar to API for ease, but owned by domain model
        public DeviceState State { get; set; } = new();
        public DeviceConfig Config { get; set; } = new();

        // UI/persistence specific fields
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Unknown";   // e.g. Online/Offline
        public string? Localization { get; set; }          // if different naming is needed

        // metadata from API
        // maps to ApiDevice.createdAt
        public DateTimeOffset? CreatedAt { get; set; }

        // Whether this device has been configured
        public bool IsConfigured { get; set; } = false;
    }

    public class DeviceState
    {
        public double Value { get; set; }
        public string? Unit { get; set; }
    }

    public class DeviceConfig
    {
        public bool ReadOnly { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Step { get; set; }
    }
}