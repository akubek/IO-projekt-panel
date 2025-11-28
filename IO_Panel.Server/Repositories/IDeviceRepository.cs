using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    public interface IDeviceRepository
    {
        Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellation = default);
        Task<Device?> GetByIdAsync(string id, CancellationToken cancellation = default);

        Task SaveConfigAsync(string deviceId, DeviceConfig config, CancellationToken cancellation = default);
        Task<DeviceConfig?> GetConfigAsync(string deviceId, CancellationToken cancellation = default);

        Task RequestStateChangeAsync(string deviceId, DeviceState newState, CancellationToken cancellation = default);
    }
}
