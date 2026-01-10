using IO_Panel.Server.Data;
using IO_Panel.Server.Data.Entities;
using IO_Panel.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Repositories.Ef;

public sealed class EfRoomRepository : IRoomRepository
{
    private readonly AppDbContext _db;
    private readonly IDeviceRepository _deviceRepository;

    public EfRoomRepository(AppDbContext db, IDeviceRepository deviceRepository)
    {
        _db = db;
        _deviceRepository = deviceRepository;
    }

    public async Task<IEnumerable<Room>> GetAllAsync()
    {
        return await _db.Rooms
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new Room
            {
                Id = r.Id,
                Name = r.Name,
                DeviceIds = new()
            })
            .ToListAsync();
    }

    public async Task<Room?> GetByIdAsync(Guid id)
    {
        return await _db.Rooms
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new Room
            {
                Id = r.Id,
                Name = r.Name,
                DeviceIds = new()
            })
            .SingleOrDefaultAsync();
    }

    public async Task AddAsync(Room room)
    {
        if (room.Id == Guid.Empty)
        {
            room.Id = Guid.NewGuid();
        }

        var entity = new RoomEntity
        {
            Id = room.Id,
            Name = room.Name
        };

        _db.Rooms.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        var entity = await _db.Rooms.SingleOrDefaultAsync(r => r.Id == room.Id);
        if (entity is null)
        {
            return;
        }

        entity.Name = room.Name;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Rooms.SingleOrDefaultAsync(r => r.Id == id);
        if (entity is null)
        {
            return;
        }

        _db.Rooms.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task AddDeviceToRoomAsync(Guid roomId, string deviceId)
    {
        var roomExists = await _db.Rooms.AsNoTracking().AnyAsync(r => r.Id == roomId);
        if (!roomExists)
        {
            return;
        }

        var device = await _db.Devices.AsNoTracking().SingleOrDefaultAsync(d => d.Id == deviceId);
        if (device is null)
        {
            return;
        }

        var exists = await _db.RoomDevices.AnyAsync(x => x.RoomId == roomId && x.DeviceId == deviceId);
        if (exists)
        {
            return;
        }

        _db.RoomDevices.Add(new RoomDeviceEntity
        {
            RoomId = roomId,
            DeviceId = deviceId
        });

        await _db.SaveChangesAsync();
    }

    public async Task RemoveDeviceFromRoomAsync(Guid roomId, string deviceId)
    {
        var link = await _db.RoomDevices
            .SingleOrDefaultAsync(x => x.RoomId == roomId && x.DeviceId == deviceId);

        if (link is null)
        {
            return;
        }

        _db.RoomDevices.Remove(link);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Device>> GetDevicesInRoomAsync(Guid roomId)
    {
        var deviceIds = await _db.RoomDevices
            .AsNoTracking()
            .Where(rd => rd.RoomId == roomId)
            .Select(rd => rd.DeviceId)
            .ToListAsync();

        if (deviceIds.Count == 0)
        {
            return Array.Empty<Device>();
        }

        var tasks = deviceIds.Select(id => _deviceRepository.GetByIdAsync(id));
        var devices = await Task.WhenAll(tasks);

        return devices.Where(d => d != null).Select(d => d!);
    }
}