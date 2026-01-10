using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories.InMemory
{
    public class AutomationRepository : IAutomationRepository
    {
        private readonly ConcurrentDictionary<Guid, Automation> _automations = new();

        public Task<IEnumerable<Automation>> GetAllAsync()
        {
            return Task.FromResult(_automations.Values.AsEnumerable());
        }

        public Task<Automation?> GetByIdAsync(Guid id)
        {
            _automations.TryGetValue(id, out var automation);
            return Task.FromResult(automation);
        }

        public Task AddAsync(Automation automation)
        {
            automation.Id = Guid.NewGuid();
            _automations[automation.Id] = automation;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Automation automation)
        {
            _automations[automation.Id] = automation;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _automations.TryRemove(id, out _);
            return Task.CompletedTask;
        }
    }
}