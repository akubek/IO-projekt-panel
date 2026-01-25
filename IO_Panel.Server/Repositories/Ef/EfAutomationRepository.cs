using System.Text.Json;
using IO_Panel.Server.Data;
using IO_Panel.Server.Data.Entities;
using IO_Panel.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Repositories.Ef;

/// <summary>
/// EF Core-backed automation repository.
/// Automations store trigger/action definitions as JSON for schema flexibility.
/// </summary>
public sealed class EfAutomationRepository : IAutomationRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;

    public EfAutomationRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns all automations (deserializing trigger/action JSON into domain models).
    /// </summary>
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
                Trigger = SafeDeserialize(a.TriggerJson, new AutomationTrigger()),
                Action = SafeDeserialize(a.ActionJson, new AutomationAction())
            })
            .ToListAsync();
    }

    /// <summary>
    /// Returns an automation by id (deserializing trigger/action JSON into domain models).
    /// </summary>
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
                Trigger = SafeDeserialize(a.TriggerJson, new AutomationTrigger()),
                Action = SafeDeserialize(a.ActionJson, new AutomationAction())
            })
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Creates an automation record. Generates an id when not provided.
    /// </summary>
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
            TriggerJson = JsonSerializer.Serialize(automation.Trigger ?? new AutomationTrigger(), JsonOptions),
            ActionJson = JsonSerializer.Serialize(automation.Action ?? new AutomationAction(), JsonOptions)
        };

        _db.Automations.Add(entity);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an automation record (including serialized trigger/action JSON).
    /// </summary>
    public async Task UpdateAsync(Automation automation)
    {
        var entity = await _db.Automations.SingleOrDefaultAsync(a => a.Id == automation.Id);
        if (entity is null)
        {
            return;
        }

        entity.Name = automation.Name;
        entity.IsEnabled = automation.IsEnabled;
        entity.TriggerJson = JsonSerializer.Serialize(automation.Trigger ?? new AutomationTrigger(), JsonOptions);
        entity.ActionJson = JsonSerializer.Serialize(automation.Action ?? new AutomationAction(), JsonOptions);

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes an automation record.
    /// </summary>
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

    /// <summary>
    /// Defensive JSON deserialization (returns a fallback value on invalid/missing JSON).
    /// </summary>
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