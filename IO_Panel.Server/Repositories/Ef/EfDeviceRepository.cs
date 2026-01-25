using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IO_Panel.Server.Data;
using IO_Panel.Server.Data.Entities;
using IO_Panel.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Repositories.Ef;

/// <summary>
/// EF Core-backed device repository.
/// Persists configured devices locally (SQLite) and optionally enriches them with live state from the external simulator API.
/// Also stores and queries device state history points.
/// </summary>
public sealed class EfDeviceRepository : IDeviceRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;
    private readonly IDeviceApiClient _deviceApiClient;

    public EfDeviceRepository(AppDbContext db, IDeviceApiClient deviceApiClient)
    {
        _db = db;
        _deviceApiClient = deviceApiClient;
    }

    /// <summary>
    /// Returns devices configured in the panel DB and enriches them with live data from the simulator when available.
    /// If the simulator is unreachable, devices are returned as Offline.
    /// </summary>
    public async Task<IEnumerable<Device>> GetConfiguredDevicesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _db.Devices
            .AsNoTracking()
            .OrderBy(d => d.DisplayName)
            .ToListAsync(cancellationToken);

        Dictionary<string, ApiDevice> apiDevices;

        try
        {
            apiDevices = (await _deviceApiClient.GetAllAsync(cancellationToken))
                .ToDictionary(d => d.Id);
        }
        catch (HttpRequestException)
        {
            apiDevices = new Dictionary<string, ApiDevice>(StringComparer.OrdinalIgnoreCase);
        }

        var result = new List<Device>(entities.Count);

        foreach (var entity in entities)
        {
            var domain = ToDomain(entity);

            if (apiDevices.TryGetValue(entity.Id, out var api))
            {
                domain.State = new DeviceState
                {
                    Value = api.State?.Value ?? domain.State.Value,
                    Unit = api.State?.Unit ?? domain.State.Unit
                };

                domain.Malfunctioning = api.Malfunctioning;
                domain.Status = "Online";
                domain.LastSeen = DateTimeOffset.UtcNow;
            }
            else
            {
                domain.Status = "Offline";
                domain.Malfunctioning = true;
            }

            result.Add(domain);
        }

        return result;
    }

    /// <summary>
    /// Returns devices from the external simulator that are not yet configured in the panel DB.
    /// </summary>
    public async Task<IEnumerable<ApiDevice>> GetUnconfiguredDevicesAsync(CancellationToken cancellationToken = default)
    {
        var apiDevices = await _deviceApiClient.GetAllAsync(cancellationToken);
        var configuredIds = await _db.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(cancellationToken);
        var configuredSet = new HashSet<string>(configuredIds);

        return apiDevices.Where(d => !configuredSet.Contains(d.Id));
    }

    /// <summary>
    /// Returns a single configured device from the DB and enriches it with a live simulator lookup when possible.
    /// </summary>
    public async Task<Device?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Devices
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var domain = ToDomain(entity);

        ApiDevice? apiDevice = null;

        try
        {
            apiDevice = await _deviceApiClient.GetByIdAsync(id, cancellationToken);
        }
        catch (HttpRequestException)
        {
            apiDevice = null;
        }

        if (apiDevice != null)
        {
            domain.State = new DeviceState
            {
                Value = apiDevice.State?.Value ?? domain.State.Value,
                Unit = apiDevice.State?.Unit ?? domain.State.Unit
            };

            domain.Malfunctioning = apiDevice.Malfunctioning;
            domain.Status = "Online";
            domain.LastSeen = DateTimeOffset.UtcNow;
        }
        else
        {
            domain.Status = "Offline";
        }

        return domain;
    }

    /// <summary>
    /// Adds a new configured device (or updates an existing record with the same id).
    /// </summary>
    public async Task<Device> AddAsync(Device device, CancellationToken cancellationToken = default)
    {
        var entity = ToEntity(device);

        var existing = await _db.Devices.SingleOrDefaultAsync(d => d.Id == device.Id, cancellationToken);
        if (existing is null)
        {
            _db.Devices.Add(entity);
        }
        else
        {
            Copy(entity, existing);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return device;
    }

    /// <summary>
    /// Updates a configured device record in the DB (metadata + serialized state/config snapshots).
    /// </summary>
    public async Task UpdateAsync(Device device, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Devices.SingleOrDefaultAsync(d => d.Id == device.Id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        entity.DisplayName = device.DisplayName;
        entity.Localization = device.Localization;
        entity.LastSeen = device.LastSeen;
        entity.Status = device.Status;
        entity.Malfunctioning = device.Malfunctioning;
        entity.ConfiguredAt = device.ConfiguredAt;
        entity.CreatedAt = device.CreatedAt;

        entity.DeviceName = device.DeviceName;
        entity.Type = device.Type;
        entity.Location = device.Location;
        entity.Description = device.Description;

        entity.StateJson = JsonSerializer.Serialize(device.State, JsonOptions);
        entity.ConfigJson = JsonSerializer.Serialize(device.Config, JsonOptions);

        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a configured device record (cascades to room links and history via FK delete rules).
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Devices.SingleOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _db.Devices.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Queries device history points in a time range (ascending time order) with a maximum row limit.
    /// </summary>
    public async Task<IReadOnlyList<DeviceStateHistoryPoint>> GetDeviceHistoryAsync(
        string deviceId,
        DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 2000);

        var fromUtc = from.UtcDateTime;
        var toUtc = to.UtcDateTime;

        var rows = await _db.DeviceStateHistory
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId && x.RecordedAtUtc >= fromUtc && x.RecordedAtUtc <= toUtc)
            .OrderByDescending(x => x.Id)
            .Take(limit)
            .Select(x => new DeviceStateHistoryPoint(
                new DateTimeOffset(x.RecordedAtUtc, TimeSpan.Zero),
                x.Value,
                x.Unit))
            .ToListAsync(cancellationToken);

        rows.Reverse();
        return rows;
    }

    /// <summary>
    /// Adds a device history point with simple de-duplication (same value/unit within a short time window).
    /// </summary>
    public async Task AddDeviceHistoryPointAsync(
        string deviceId,
        double value,
        string? unit,
        DateTimeOffset recordedAt,
        CancellationToken cancellationToken = default)
    {
        var exists = await _db.Devices
            .AsNoTracking()
            .AnyAsync(d => d.Id == deviceId, cancellationToken);

        if (!exists)
        {
            return;
        }

        var dedupeWindow = TimeSpan.FromSeconds(10);

        var last = await _db.DeviceStateHistory
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId)
            .OrderByDescending(x => x.Id)
            .Select(x => new { x.RecordedAtUtc, x.Value, x.Unit })
            .FirstOrDefaultAsync(cancellationToken);

        var recordedAtUtc = recordedAt.UtcDateTime;

        if (last is not null)
        {
            var sameUnit = string.Equals(last.Unit, unit, StringComparison.OrdinalIgnoreCase);
            var sameValue = Math.Abs(last.Value - value) <= 1e-9;
            var withinWindow = (recordedAtUtc - last.RecordedAtUtc) <= dedupeWindow;

            if (sameUnit && sameValue && withinWindow)
            {
                return;
            }
        }

        _db.DeviceStateHistory.Add(new DeviceStateHistoryEntity
        {
            DeviceId = deviceId,
            RecordedAtUtc = recordedAtUtc,
            Value = value,
            Unit = unit
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Maps EF entity to domain model and deserializes state/config JSON blobs safely.
    /// </summary>
    private static Device ToDomain(DeviceEntity entity)
    {
        var state = SafeDeserialize(entity.StateJson, new DeviceState());
        var config = SafeDeserialize(entity.ConfigJson, new DeviceConfig());

        return new Device
        {
            Id = entity.Id,
            DeviceName = entity.DeviceName,
            Type = entity.Type,
            Location = entity.Location,
            Description = entity.Description,

            Status = entity.Status,
            DisplayName = entity.DisplayName,
            Localization = entity.Localization,

            LastSeen = entity.LastSeen,
            ConfiguredAt = entity.ConfiguredAt,
            CreatedAt = entity.CreatedAt,
            Malfunctioning = entity.Malfunctioning,

            State = state,
            Config = config
        };
    }

    /// <summary>
    /// Maps domain model to EF entity and serializes state/config into JSON blobs.
    /// </summary>
    private static DeviceEntity ToEntity(Device device)
    {
        return new DeviceEntity
        {
            Id = device.Id,

            DeviceName = device.DeviceName,
            Type = device.Type,
            Location = device.Location,
            Description = device.Description,

            Status = device.Status,
            DisplayName = device.DisplayName,
            Localization = device.Localization,

            LastSeen = device.LastSeen,
            ConfiguredAt = device.ConfiguredAt,
            CreatedAt = device.CreatedAt,
            Malfunctioning = device.Malfunctioning,

            StateJson = JsonSerializer.Serialize(device.State, JsonOptions),
            ConfigJson = JsonSerializer.Serialize(device.Config, JsonOptions)
        };
    }

    /// <summary>
    /// Copies mapped entity values into an existing tracked entity (used for upsert-style add).
    /// </summary>
    private static void Copy(DeviceEntity source, DeviceEntity target)
    {
        target.DeviceName = source.DeviceName;
        target.Type = source.Type;
        target.Location = source.Location;
        target.Description = source.Description;

        target.Status = source.Status;
        target.DisplayName = source.DisplayName;
        target.Localization = source.Localization;

        target.LastSeen = source.LastSeen;
        target.ConfiguredAt = source.ConfiguredAt;
        target.CreatedAt = source.CreatedAt;
        target.Malfunctioning = source.Malfunctioning;

        target.StateJson = source.StateJson;
        target.ConfigJson = source.ConfigJson;
    }

    /// <summary>
    /// Defensive JSON deserialization (returns a fallback value on invalid/missing JSON).
    /// </summary>
    private static T SafeDeserialize<T>(string json, T fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }
}