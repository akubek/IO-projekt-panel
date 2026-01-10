using System.Text.Json;
using IO_Panel.Server.Data;
using IO_Panel.Server.Data.Entities;
using IO_Panel.Server.Mappers;
using IO_Panel.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Repositories.Ef;

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

    public async Task<IEnumerable<Device>> GetConfiguredDevicesAsync(CancellationToken cancellationToken = default)
    {
        // Load persisted configured devices
        var entities = await _db.Devices
            .AsNoTracking()
            .OrderBy(d => d.DisplayName)
            .ToListAsync(cancellationToken);

        // Fetch external devices once for enrichment
        var apiDevices = (await _deviceApiClient.GetAllAsync(cancellationToken))
            .ToDictionary(d => d.Id);

        var result = new List<Device>(entities.Count);

        foreach (var entity in entities)
        {
            var domain = ToDomain(entity);

            if (apiDevices.TryGetValue(entity.Id, out var api))
            {
                // Merge live state/malfunctioning from API into the persisted device
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
            }

            result.Add(domain);
        }

        return result;
    }

    public async Task<IEnumerable<ApiDevice>> GetUnconfiguredDevicesAsync(CancellationToken cancellationToken = default)
    {
        var apiDevices = await _deviceApiClient.GetAllAsync(cancellationToken);
        var configuredIds = await _db.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(cancellationToken);
        var configuredSet = new HashSet<string>(configuredIds);

        return apiDevices.Where(d => !configuredSet.Contains(d.Id));
    }

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

        var apiDevice = await _deviceApiClient.GetByIdAsync(id, cancellationToken);
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

    public async Task<Device> AddAsync(Device device, CancellationToken cancellationToken = default)
    {
        // Id is external API id, so do not generate it.
        var entity = ToEntity(device);

        // Upsert style: if exists, update; otherwise insert.
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