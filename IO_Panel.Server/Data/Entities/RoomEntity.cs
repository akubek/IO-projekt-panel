namespace IO_Panel.Server.Data.Entities;

/// <summary>
/// Persisted room definition. Rooms group configured devices for display and bulk management.
/// </summary>
public sealed class RoomEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Join entities linking this room to devices.
    /// </summary>
    public List<RoomDeviceEntity> RoomDevices { get; set; } = new();
}