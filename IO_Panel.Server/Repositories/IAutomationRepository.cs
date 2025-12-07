using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    public interface IAutomationRepository
    {
        Task<IEnumerable<Automation>> GetAllAsync();
        Task<Automation?> GetByIdAsync(Guid id);
        Task AddAsync(Automation automation);
        Task UpdateAsync(Automation automation);
        Task DeleteAsync(Guid id);
    }
}