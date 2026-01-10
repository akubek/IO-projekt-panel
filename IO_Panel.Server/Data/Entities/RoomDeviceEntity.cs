namespace IO_Panel.Server.Data.Entities;

public sealed class RoomDeviceEntity
{
    public Guid RoomId { get; set; }
    public RoomEntity Room { get; set; } = default!;

    public string DeviceId { get; set; } = default!;
    public DeviceEntity Device { get; set; } = default!;
}