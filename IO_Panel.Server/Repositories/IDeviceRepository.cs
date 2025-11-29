using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    // Domain-level repository used by controllers/services — returns domain Device
    public interface IDeviceRepository
    {
        Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellation = default);
        Task<Device?> GetByIdAsync(string id, CancellationToken cancellation = default);

        // Persist/read only the configuration (or full device if you prefer)
        Task SaveConfigAsync(string deviceId, DeviceConfig config, CancellationToken cancellation = default);
        Task<DeviceConfig?> GetConfigAsync(string deviceId, CancellationToken cancellation = default);

        // Domain-level request to change state (works with your domain DeviceState)
        Task RequestStateChangeAsync(string deviceId, DeviceState newState, CancellationToken cancellation = default);

        // Add or update a device in the repository
        Task AddAsync(Device device, CancellationToken cancellation = default);
    }
}
