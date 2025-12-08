using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories.Entities;

namespace IO_Panel.Server.Repositories;

public interface IDeviceRepository
{
    Task<IEnumerable<Device>> GetConfiguredDevicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiDevice>> GetUnconfiguredDevicesAsync(CancellationToken cancellationToken = default);
    Task<Device?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Device> AddAsync(ApiDevice device, string name, CancellationToken cancellationToken = default);
    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
