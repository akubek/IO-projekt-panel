using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    public interface IDeviceConfigRepository
    {
        Task<DeviceConfig?> LoadConfigAsync(string deviceId, CancellationToken cancellation = default);
        Task SaveConfigAsync(string deviceId, DeviceConfig config, CancellationToken cancellation = default);

        //enumerate all device ids that have persisted configuration
        Task<IEnumerable<string>> GetAllDeviceIdsAsync(CancellationToken cancellation = default);
    }
}