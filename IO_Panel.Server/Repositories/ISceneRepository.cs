using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    public interface ISceneRepository
    {
        Task<IEnumerable<Scene>> GetAllAsync();
        Task<Scene?> GetByIdAsync(Guid id);
        Task AddAsync(Scene scene);
        Task UpdateAsync(Scene scene);
        Task DeleteAsync(Guid id);
    }
}