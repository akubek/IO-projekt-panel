using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    public class InMemoryDeviceConfigRepository : IDeviceConfigRepository
    {
        private readonly ConcurrentDictionary<string, DeviceConfig> _configs = new();

        public Task<DeviceConfig?> LoadConfigAsync(string deviceId, CancellationToken cancellation = default) =>
            Task.FromResult(_configs.TryGetValue(deviceId, out var cfg) ? cfg : null);

        public Task SaveConfigAsync(string deviceId, DeviceConfig config, CancellationToken cancellation = default)
        {
            _configs[deviceId] = config;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetAllDeviceIdsAsync(CancellationToken cancellation = default) =>
            Task.FromResult<IEnumerable<string>>(_configs.Keys);
    }
}