using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Data.Entities;

public sealed class DeviceEntity
{
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

    // Persist nested objects as JSON blobs (simple for SQLite)
    public string StateJson { get; set; } = "{}";
    public string ConfigJson { get; set; } = "{}";

    public List<RoomDeviceEntity> RoomDevices { get; set; } = new();

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