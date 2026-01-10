using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IO_Panel.Server.Mappers;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories.InMemory;

public class DeviceRepository : IDeviceRepository
{
    private static readonly List<Device> _devices = new();
    private readonly IDeviceApiClient _deviceApiClient;
    private const double StateEqualityEpsilon = 1e-6;

    // In-memory store for configs, replacing the separate repository
    private readonly ConcurrentDictionary<string, DeviceConfig> _configs = new();
    private readonly ILogger<DeviceRepository> _logger;

    public DeviceRepository(IDeviceApiClient deviceApiClient, ILogger<DeviceRepository> logger)
    {
        _logger = logger;
        _deviceApiClient = deviceApiClient;
    }

    public async Task<IEnumerable<Device>> GetConfiguredDevicesAsync(CancellationToken cancellationToken = default)
    {
        var apiDevices = (await _deviceApiClient.GetAllAsync()).ToDictionary(d => d.Id);

        //try to update all devices before returning them
        foreach (var device in _devices)
        {
            if (apiDevices.TryGetValue(device.Id, out var apiDevice))
            {
                device.UpdateFromApiDevice(apiDevice);
            }
            else
            {
                device.Status = "Offline";
            }
        }

        return await Task.FromResult(_devices);
    }

    public async Task<IEnumerable<ApiDevice>> GetUnconfiguredDevicesAsync(CancellationToken cancellationToken = default)
    {
        var apiDevices = await _deviceApiClient.GetAllAsync(cancellationToken);
        var configuredDeviceIds = new HashSet<string>(_devices.Select(d => d.Id));

        //return only devices that are not already configured in the system
        return apiDevices.Where(d => !configuredDeviceIds.Contains(d.Id));
    }

    public async Task<Device?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device == null)
        {
            return null;
        }

        var apiDevice = await _deviceApiClient.GetByIdAsync(id, cancellationToken);
        if (apiDevice != null)
        {
            device.UpdateFromApiDevice(apiDevice);
        }
        else
        {
            device.Status = "Offline";
        }

        return device;
    }

    public Task<Device> AddAsync(Device device, CancellationToken cancellationToken = default)
    {
        _devices.Add(device);
        return Task.FromResult(device);
    }

    public Task UpdateAsync(Device device, CancellationToken cancellationToken = default)
    {
        var existingDevice = _devices.FirstOrDefault(d => d.Id == device.Id);
        if (existingDevice != null)
        {
            existingDevice.DisplayName = device.DisplayName;
            existingDevice.Status = device.Status;
            existingDevice.LastSeen = device.LastSeen;
            existingDevice.Localization = device.Localization;
        }
        return Task.CompletedTask;
    }


    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device != null)
        {
            _devices.Remove(device);
        }
        return Task.CompletedTask;
    }
}