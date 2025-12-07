using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    // Domain-level repository used by controllers/services — returns domain Device
    public interface IDeviceRepository
    {
        Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellation = default);
        Task<Device?> GetByIdAsync(string id, CancellationToken cancellation = default);
        Task AddAsync(Device device, CancellationToken cancellation = default);
        Task RequestStateChangeAsync(string deviceId, DeviceState newState, CancellationToken cancellation = default);

        Task SaveConfigAsync(string deviceId, DeviceConfig config, CancellationToken cancellation = default);
        Task<DeviceConfig?> GetConfigAsync(string deviceId, CancellationToken cancellation = default);
        Task<IEnumerable<string>> GetAllDeviceIdsAsync(CancellationToken cancellation = default);
    }
}
