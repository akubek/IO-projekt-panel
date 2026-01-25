namespace IO_Panel.Server.Models
{
    /// <summary>
    /// Domain model representing a configured device shown in the panel UI.
    /// Combines simulator-provided fields (type/state/config) with panel-specific metadata (display name, status, timestamps).
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Device identifier (string; expected to be a GUID string in this solution).
        /// </summary>
        public string Id { get; set; } = default!;

        public string DeviceName { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string Location { get; set; } = default!;
        public string Description { get; set; } = default!;

        /// <summary>
        /// Current device state snapshot (value + optional unit).
        /// </summary>
        public DeviceState State { get; set; } = new();

        /// <summary>
        /// Device control configuration (range, step, read-only constraints).
        /// </summary>
        public DeviceConfig Config { get; set; } = new();

        public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// UI-facing status string (e.g., Online/Offline).
        /// </summary>
        public string Status { get; set; } = "Offline";

        /// <summary>
        /// Name shown in the panel UI (can differ from the simulator device name).
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Optional custom label/translation field used by the UI.
        /// </summary>
        public string? Localization { get; set; }

        /// <summary>
        /// Timestamp when the device was configured/imported in the panel.
        /// </summary>
        public DateTimeOffset? ConfiguredAt { get; set; }

        /// <summary>
        /// Metadata from the simulator (maps to ApiDevice.createdAt).
        /// </summary>
        public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Indicates the device is malfunctioning according to the simulator/device update stream.
        /// </summary>
        public bool Malfunctioning { get; set; }
    }

    /// <summary>
    /// Represents the current numeric state of a device (value + optional unit).
    /// </summary>
    public class DeviceState
    {
        public double Value { get; set; }
        public string? Unit { get; set; }
    }

    /// <summary>
    /// Device control configuration used by the UI to render controls (toggle/slider) and validate bounds.
    /// </summary>
    public class DeviceConfig
    {
        public bool ReadOnly { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Step { get; set; }
    }
}