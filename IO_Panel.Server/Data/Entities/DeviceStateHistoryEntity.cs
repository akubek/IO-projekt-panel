using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Data.Entities;

public sealed class DeviceStateHistoryEntity
{
    public long Id { get; set; }

    public string DeviceId { get; set; } = default!;
    public DeviceEntity Device { get; set; } = default!;

    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

    public double Value { get; set; }
    public string? Unit { get; set; }

    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceStateHistoryEntity>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<DeviceStateHistoryEntity>()
            .Property(x => x.RecordedAtUtc);

        modelBuilder.Entity<DeviceStateHistoryEntity>()
            .HasIndex(x => new { x.DeviceId, x.RecordedAtUtc });

        modelBuilder.Entity<DeviceStateHistoryEntity>()
            .HasOne(x => x.Device)
            .WithMany()
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}