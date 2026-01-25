using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories;

/// <summary>
/// Abstraction for configured device persistence and queries.
/// Includes device configuration data and time-series device state history.
/// </summary>
public interface IDeviceRepository
{
    /// <summary>
    /// Returns devices configured in the panel database (optionally enriched by the repository implementation).
    /// </summary>
    Task<IEnumerable<Device>> GetConfiguredDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns simulator devices that are not yet configured in the panel database.
    /// </summary>
    Task<IEnumerable<ApiDevice>> GetUnconfiguredDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single configured device by id.
    /// </summary>
    Task<Device?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new configured device record.
    /// </summary>
    Task<Device> AddAsync(Device device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing configured device record.
    /// </summary>
    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a configured device record by id.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries device state history points in a given time range with a maximum row limit.
    /// </summary>
    Task<IReadOnlyList<DeviceStateHistoryPoint>> GetDeviceHistoryAsync(
        string deviceId,
        DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a device state history point to the store.
    /// </summary>
    Task AddDeviceHistoryPointAsync(
        string deviceId,
        double value,
        string? unit,
        DateTimeOffset recordedAt,
        CancellationToken cancellationToken = default);
}
