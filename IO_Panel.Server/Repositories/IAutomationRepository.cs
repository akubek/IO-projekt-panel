using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    /// <summary>
    /// Abstraction for automation persistence and retrieval.
    /// </summary>
    public interface IAutomationRepository
    {
        Task<IEnumerable<Automation>> GetAllAsync();
        Task<Automation?> GetByIdAsync(Guid id);
        Task AddAsync(Automation automation);
        Task UpdateAsync(Automation automation);
        Task DeleteAsync(Guid id);
    }
}