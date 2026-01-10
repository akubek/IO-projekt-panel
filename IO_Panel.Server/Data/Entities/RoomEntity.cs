namespace IO_Panel.Server.Data.Entities;

public sealed class RoomEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<RoomDeviceEntity> RoomDevices { get; set; } = new();
}