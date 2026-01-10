using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories;

public interface IDeviceRepository
{
    Task<IEnumerable<Device>> GetConfiguredDevicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiDevice>> GetUnconfiguredDevicesAsync(CancellationToken cancellationToken = default);
    Task<Device?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Device> AddAsync(Device device, CancellationToken cancellationToken = default);
    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeviceStateHistoryPoint>> GetDeviceHistoryAsync(
        string deviceId,
        DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        CancellationToken cancellationToken = default);

    Task AddDeviceHistoryPointAsync(
        string deviceId,
        double value,
        string? unit,
        DateTimeOffset recordedAt,
        CancellationToken cancellationToken = default);
}
