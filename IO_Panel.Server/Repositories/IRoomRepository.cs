using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(Guid id);
        Task AddAsync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(Guid id);
    }
}