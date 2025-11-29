using System.Collections.Concurrent;
using System.Linq;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    // Simple in-memory implementation for development/tests.
    // Use AddOrUpdate to seed devices; register as singleton to preserve state across requests.
    public class InMemoryDeviceRepository : IDeviceRepository
    {
        private readonly ConcurrentDictionary<string, Device> _devices = new();
        private readonly ConcurrentDictionary<string, DeviceConfig> _configs = new();

        public InMemoryDeviceRepository(IEnumerable<Device>? seed = null)
        {
            if (seed is not null)
            {
                foreach (var d in seed)
                {
                    _devices[d.Id] = d;
                    _configs[d.Id] = d.Config;
                }
            }
        }

        public Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellation = default) =>
            Task.FromResult(_devices.Values.AsEnumerable());

        public Task<Device?> GetByIdAsync(string id, CancellationToken cancellation = default) =>
            Task.FromResult(_devices.TryGetValue(id, out var device) ? device : null);

        public Task SaveConfigAsync(string deviceId, DeviceConfig config, CancellationToken cancellation = default)
        {
            _configs[deviceId] = config;

            // Keep in-memory device config in sync when device exists
            if (_devices.TryGetValue(deviceId, out var device))
            {
                device.Config = config;
            }

            return Task.CompletedTask;
        }

        public Task<DeviceConfig?> GetConfigAsync(string deviceId, CancellationToken cancellation = default) =>
            Task.FromResult(_configs.TryGetValue(deviceId, out var cfg) ? cfg : null);

        public Task RequestStateChangeAsync(string deviceId, DeviceState newState, CancellationToken cancellation = default)
        {
            cancellation.ThrowIfCancellationRequested();

            if (!_devices.TryGetValue(deviceId, out var existingDevice))
            {
                throw new KeyNotFoundException($"Device '{deviceId}' not found.");
            }

            // Prefer persisted override config if present
            var config = _configs.TryGetValue(deviceId, out var persisted) ? persisted : existingDevice.Config;

            if (config?.ReadOnly == true)
            {
                throw new InvalidOperationException($"Device '{deviceId}' is read-only and cannot accept state changes.");
            }

            // Thread-safe update of the device state and metadata
            _devices.AddOrUpdate(deviceId, existingDevice, (key, device) =>
            {
                device.State = newState;
                device.LastSeen = DateTime.UtcNow;
                device.Status = "Online";
                return device;
            });

            return Task.CompletedTask;
        }

        // Add or update device
        public Task AddAsync(Device device, CancellationToken cancellation = default)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (string.IsNullOrWhiteSpace(device.Id)) device.Id = Guid.NewGuid().ToString();

            _devices.AddOrUpdate(device.Id, device, (_, __) => device);

            if (device.Config != null)
            {
                _configs[device.Id] = device.Config;
            }

            return Task.CompletedTask;
        }

        // Helpers for tests/dev
        public void AddOrUpdate(Device device) => _devices.AddOrUpdate(device.Id, device, (_, __) => device);
        public bool Remove(string id) => _devices.TryRemove(id, out _);
    }
}