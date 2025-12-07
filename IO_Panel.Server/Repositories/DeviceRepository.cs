using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IO_Panel.Server.Mappers;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories.Entities;
using Microsoft.Extensions.Logging;

namespace IO_Panel.Server.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly IDeviceApiClient _api;
        private readonly ILogger<DeviceRepository> _logger;
        private const double StateEqualityEpsilon = 1e-6;

        // In-memory store for configs, replacing the separate repository
        private readonly ConcurrentDictionary<string, DeviceConfig> _configs = new();

        public DeviceRepository(IDeviceApiClient api, ILogger<DeviceRepository> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellation = default)
        {
            // 1) fetch devices reported by external API
            var apiList = await _api.GetAllAsync(cancellation).ConfigureAwait(false);

            // map API -> domain devices and compute stable ids
            var apiEntries = new List<(string Id, Device Domain)>();
            foreach (var api in apiList)
            {
                cancellation.ThrowIfCancellationRequested();

                // require an explicit id from API;
                var id = TryGetApiId(api);
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogError("External API returned a device without an 'id' (name='{Name}', location='{Location}'). Skipping device.", api?.Name, api?.Location);
                    continue; // skip devices that don't supply an id
                }

                var domain = DeviceMapper.ToDomain(api, id);
                apiEntries.Add((Id: id, Domain: domain));
            }

            // 2) load configs for reported devices concurrently (best-effort)
            var loadTasks = apiEntries.Select(async e =>
            {
                cancellation.ThrowIfCancellationRequested();
                var persisted = await GetConfigAsync(e.Id, cancellation).ConfigureAwait(false);
                if (persisted != null)
                {
                    e.Domain.Config = persisted;
                    e.Domain.IsConfigured = true;
                }
                else
                {
                    e.Domain.IsConfigured = false;
                }
                return e.Domain;
            });

            var reportedDevices = (await Task.WhenAll(loadTasks).ConfigureAwait(false)).ToList();

            // 3) find persisted (remembered) device ids that are not reported by the API -> disconnected
            var persistedIds = await GetAllDeviceIdsAsync(cancellation).ConfigureAwait(false);
            var reportedIds = new HashSet<string>(reportedDevices.Select(d => d.Id));
            var disconnected = new List<Device>();

            foreach (var pid in persistedIds)
            {
                cancellation.ThrowIfCancellationRequested();
                if (reportedIds.Contains(pid)) continue; // already included

                try
                {
                    var cfg = await GetConfigAsync(pid, cancellation).ConfigureAwait(false);
                    var d = new Device
                    {
                        Id = pid,
                        Name = pid, // minimal info: name not available from API; UI can show id or allow editing
                        Type = string.Empty,
                        Location = string.Empty,
                        Description = "Remembered, not reported by API",
                        State = new DeviceState { Value = 0, Unit = null },
                        Config = cfg ?? new DeviceConfig(),
                        LastSeen = DateTime.MinValue,
                        Status = "Disconnected",
                        IsConfigured = cfg != null
                    };

                    disconnected.Add(d);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error creating disconnected device entry for '{DeviceId}'", pid);
                }
            }

            // 4) return reported devices (+ disconnected remembered devices)
            var result = reportedDevices.Concat(disconnected).ToList();
            return result;
        }

        public async Task<Device?> GetByIdAsync(string id, CancellationToken cancellation = default)
        {
            var api = await _api.GetByIdAsync(id, cancellation);
            if (api == null)
            {
                // If API does not report device, but we have a persisted config, return a "disconnected" remembered device
                var persisted = await GetConfigAsync(id, cancellation).ConfigureAwait(false);
                if (persisted != null)
                {
                    return new Device
                    {
                        Id = id,
                        Name = id,
                        Type = string.Empty,
                        Location = string.Empty,
                        Description = "Remembered, not reported by API",
                        State = new DeviceState { Value = 0, Unit = null },
                        Config = persisted,
                        LastSeen = DateTime.MinValue,
                        Status = "Disconnected",
                        IsConfigured = true
                    };
                }

                return null;
            }

            var domain = DeviceMapper.ToDomain(api, id);
            var domainConfig = await GetConfigAsync(id, cancellation).ConfigureAwait(false);
            if (domainConfig != null)
            {
                domain.Config = domainConfig;
                domain.IsConfigured = true;
            }
            return domain;
        }

        public Task<DeviceConfig?> GetConfigAsync(string deviceId, CancellationToken cancellation = default) =>
            Task.FromResult(_configs.TryGetValue(deviceId, out var cfg) ? cfg : null);

        public Task SaveConfigAsync(string deviceId, DeviceConfig config, CancellationToken cancellation = default)
        {
            _configs[deviceId] = config;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetAllDeviceIdsAsync(CancellationToken cancellation = default) =>
            Task.FromResult<IEnumerable<string>>(_configs.Keys);

        public async Task RequestStateChangeAsync(string deviceId, DeviceState newState, CancellationToken cancellation)
        {
            // Convert domain -> API state
            var apiState = new ApiDeviceState { Value = newState.Value, Unit = newState.Unit };

            // Send to external API
            await _api.SetStateAsync(deviceId, apiState, cancellation).ConfigureAwait(false);

            // Wait for confirmation by polling the API until the device state matches requested state or timeout
            const int maxAttempts = 20;
            const int delayMs = 500; // total ~10s
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                cancellation.ThrowIfCancellationRequested();

                var apiDevice = await _api.GetByIdAsync(deviceId, cancellation).ConfigureAwait(false);
                if (apiDevice?.State != null)
                {
                    var got = apiDevice.State;

                    // Use tolerance when comparing floating point values
                    bool valueMatches = Math.Abs(got.Value - newState.Value) <= StateEqualityEpsilon;
                    bool unitMatches = string.Equals(got.Unit, newState.Unit, StringComparison.OrdinalIgnoreCase) ||
                                       (string.IsNullOrEmpty(got.Unit) && string.IsNullOrEmpty(newState.Unit));

                    if (valueMatches && unitMatches)
                    {
                        // Optionally refresh persisted config (best-effort)
                        try
                        {
                            var persisted = await GetConfigAsync(deviceId, cancellation).ConfigureAwait(false);
                            if (persisted != null)
                            {
                                // nothing stored in-memory here, but we ensure config repository is reachable
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Could not refresh config after state change for '{DeviceId}'", deviceId);
                        }

                        // success: return
                        return;
                    }
                }

                await Task.Delay(delayMs, cancellation).ConfigureAwait(false);
            }

            _logger.LogWarning("Timed out waiting for device '{DeviceId}' to reach requested state (value={Value}, unit={Unit})",
                deviceId, newState.Value, newState.Unit);
            throw new TimeoutException($"Timed out waiting for device '{deviceId}' to reach requested state.");
        }

        public async Task AddAsync(Device device, CancellationToken cancellation = default)
        {
            // Persist the device config if available
            if (device.Config != null)
            {
                await SaveConfigAsync(device.Id, device.Config, cancellation).ConfigureAwait(false);
            }
        }

        private static string? TryGetApiId(ApiDevice api)
        {
            // If API includes an Id property in the returned object, prefer it.
            var prop = api.GetType().GetProperty("Id");
            if (prop != null)
            {
                if (prop.GetValue(api) is string s && !string.IsNullOrEmpty(s))
                    return s;
            }

            return null;
        }
    }
}