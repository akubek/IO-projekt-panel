using IO_Panel.Server.Data;
using IO_Panel.Server.Data.Entities;
using IO_Panel.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Repositories.Ef;

public sealed class EfAutomationRepository : IAutomationRepository
{
    private readonly AppDbContext _db;

    public EfAutomationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Automation>> GetAllAsync()
    {
        return await _db.Automations
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new Automation
            {
                Id = a.Id,
                Name = a.Name,
                IsEnabled = a.IsEnabled,
                LogicDefinition = a.LogicDefinition
            })
            .ToListAsync();
    }

    public async Task<Automation?> GetByIdAsync(Guid id)
    {
        return await _db.Automations
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new Automation
            {
                Id = a.Id,
                Name = a.Name,
                IsEnabled = a.IsEnabled,
                LogicDefinition = a.LogicDefinition
            })
            .SingleOrDefaultAsync();
    }

    public async Task AddAsync(Automation automation)
    {
        if (automation.Id == Guid.Empty)
        {
            automation.Id = Guid.NewGuid();
        }

        var entity = new AutomationEntity
        {
            Id = automation.Id,
            Name = automation.Name,
            IsEnabled = automation.IsEnabled,
            LogicDefinition = automation.LogicDefinition
        };

        _db.Automations.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Automation automation)
    {
        var entity = await _db.Automations.SingleOrDefaultAsync(a => a.Id == automation.Id);
        if (entity is null)
        {
            return;
        }

        entity.Name = automation.Name;
        entity.IsEnabled = automation.IsEnabled;
        entity.LogicDefinition = automation.LogicDefinition;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Automations.SingleOrDefaultAsync(a => a.Id == id);
        if (entity is null)
        {
            return;
        }

        _db.Automations.Remove(entity);
        await _db.SaveChangesAsync();
    }
}