using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    /// <summary>
    /// Abstraction for room persistence and membership management (room-device assignment).
    /// </summary>
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(Guid id);
        Task AddAsync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Adds a configured device to the specified room.
        /// </summary>
        Task AddDeviceToRoomAsync(Guid roomId, string deviceId);

        /// <summary>
        /// Removes a configured device from the specified room.
        /// </summary>
        Task RemoveDeviceFromRoomAsync(Guid roomId, string deviceId);

        /// <summary>
        /// Returns the current device snapshots that belong to the specified room.
        /// </summary>
        Task<IEnumerable<Device>> GetDevicesInRoomAsync(Guid roomId);
    }
}