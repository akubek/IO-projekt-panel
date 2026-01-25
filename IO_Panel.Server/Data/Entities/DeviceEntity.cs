using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Data.Entities;

/// <summary>
/// Persisted representation of a configured device.
/// Combines external simulator fields (type/location/state/config) with panel-specific metadata (display name, status).
/// </summary>
public sealed class DeviceEntity
{
    /// <summary>
    /// Device identifier (string; expected to be a GUID string in this solution).
    /// </summary>
    public string Id { get; set; } = default!;

    public string DeviceName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Offline";
    public string DisplayName { get; set; } = string.Empty;
    public string? Localization { get; set; }

    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ConfiguredAt { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public bool Malfunctioning { get; set; }

    /// <summary>
    /// JSON blob containing the current device state (stored as JSON to keep SQLite schema simple).
    /// </summary>
    public string StateJson { get; set; } = "{}";

    /// <summary>
    /// JSON blob containing the device configuration (stored as JSON to keep SQLite schema simple).
    /// </summary>
    public string ConfigJson { get; set; } = "{}";

    /// <summary>
    /// Join entities linking this device to rooms.
    /// </summary>
    public List<RoomDeviceEntity> RoomDevices { get; set; } = new();

    /// <summary>
    /// Model configuration for defaults / constraints.
    /// </summary>
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceEntity>()
            .Property(x => x.StateJson)
            .HasDefaultValue("{}");

        modelBuilder.Entity<DeviceEntity>()
            .Property(x => x.ConfigJson)
            .HasDefaultValue("{}");
    }
}