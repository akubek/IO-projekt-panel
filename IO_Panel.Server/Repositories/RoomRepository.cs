using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ConcurrentDictionary<Guid, Room> _rooms = new();
        private readonly IDeviceRepository _deviceRepository;

        public RoomRepository(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public Task<IEnumerable<Room>> GetAllAsync()
        {
            return Task.FromResult(_rooms.Values.AsEnumerable());
        }

        public Task<Room?> GetByIdAsync(Guid id)
        {
            _rooms.TryGetValue(id, out var room);
            return Task.FromResult(room);
        }

        public Task AddAsync(Room room)
        {
            //assigns a new unique ID and stores the room in the in-memory dictionary
            room.Id = Guid.NewGuid();
            _rooms[room.Id] = room;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Room room)
        {
            _rooms[room.Id] = room;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _rooms.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public async Task AddDeviceToRoomAsync(Guid roomId, string deviceId)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                if (!room.DeviceIds.Contains(deviceId))
                {
                    room.DeviceIds.Add(deviceId);
                    await UpdateAsync(room);
                }
            }
        }

        public async Task RemoveDeviceFromRoomAsync(Guid roomId, string deviceId)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                room.DeviceIds.Remove(deviceId);
                await UpdateAsync(room);
            }
        }

        public async Task<IEnumerable<Device>> GetDevicesInRoomAsync(Guid roomId)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                return Enumerable.Empty<Device>();
            }

            var deviceTasks = room.DeviceIds.Select(deviceId => _deviceRepository.GetByIdAsync(deviceId));
            var devices = await Task.WhenAll(deviceTasks);
            
            return devices.Where(d => d != null).Select(d => d!);
        }
    }
}