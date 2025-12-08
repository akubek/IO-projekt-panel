using IO_Panel.Server.Mappers;
using IO_Panel.Server.Repositories.Entities;

namespace IO_Panel.Server.Models
{
    // Domain / view model used by controllers, UI and persistence layers
    public class Device
    {
        // local identifier based on ApiDevice.id
        public string Id { get; set; } = default!;

        public string DeviceName { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string Location { get; set; } = default!;
        public string Description { get; set; } = default!;

        // nested state and config based on ApiDevice structure
        public DeviceState State { get; set; } = new();
        public DeviceConfig Config { get; set; } = new();

        // UI/persistence specific fields
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Offline";   // e.g. Online/Offline low battery, etc.
        public string DisplayName { get; set; } = string.Empty; //name displayed in the IoT Panel
        public string? Localization { get; set; }          // if different naming is needed, not used yet

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