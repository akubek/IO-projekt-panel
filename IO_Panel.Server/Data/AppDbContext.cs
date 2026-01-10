using IO_Panel.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<DeviceEntity> Devices => Set<DeviceEntity>();
    public DbSet<DeviceStateHistoryEntity> DeviceStateHistory => Set<DeviceStateHistoryEntity>();
    public DbSet<RoomEntity> Rooms => Set<RoomEntity>();
    public DbSet<RoomDeviceEntity> RoomDevices => Set<RoomDeviceEntity>();

    public DbSet<SceneEntity> Scenes => Set<SceneEntity>();
    public DbSet<SceneActionEntity> SceneActions => Set<SceneActionEntity>();

    public DbSet<AutomationEntity> Automations => Set<AutomationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DeviceEntity>()
            .HasKey(d => d.Id);

        DeviceStateHistoryEntity.Configure(modelBuilder);

        modelBuilder.Entity<RoomEntity>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<RoomDeviceEntity>()
            .HasKey(x => new { x.RoomId, x.DeviceId });

        modelBuilder.Entity<RoomDeviceEntity>()
            .HasOne(x => x.Room)
            .WithMany(r => r.RoomDevices)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoomDeviceEntity>()
            .HasOne(x => x.Device)
            .WithMany(d => d.RoomDevices)
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SceneEntity>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<SceneActionEntity>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<SceneActionEntity>()
            .HasOne(a => a.Scene)
            .WithMany(s => s.Actions)
            .HasForeignKey(a => a.SceneId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AutomationEntity>()
            .HasKey(a => a.Id);
    }
}