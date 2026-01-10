using IO_Panel.Server.Data;
using IO_Panel.Server.Data.Entities;
using IO_Panel.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Repositories.Ef;

public sealed class EfSceneRepository : ISceneRepository
{
    private readonly AppDbContext _db;

    public EfSceneRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Scene>> GetAllAsync()
    {
        return await _db.Scenes
            .AsNoTracking()
            .Include(s => s.Actions)
            .OrderBy(s => s.Name)
            .Select(s => new Scene
            {
                Id = s.Id,
                Name = s.Name,
                IsPublic = s.IsPublic,
                Actions = s.Actions
                    .OrderBy(a => a.Id)
                    .Select(a => new SceneAction
                    {
                        DeviceId = a.DeviceId,
                        TargetState = new DeviceState
                        {
                            Value = a.TargetValue,
                            Unit = a.TargetUnit
                        }
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<Scene?> GetByIdAsync(Guid id)
    {
        return await _db.Scenes
            .AsNoTracking()
            .Include(s => s.Actions)
            .Where(s => s.Id == id)
            .Select(s => new Scene
            {
                Id = s.Id,
                Name = s.Name,
                IsPublic = s.IsPublic,
                Actions = s.Actions
                    .OrderBy(a => a.Id)
                    .Select(a => new SceneAction
                    {
                        DeviceId = a.DeviceId,
                        TargetState = new DeviceState
                        {
                            Value = a.TargetValue,
                            Unit = a.TargetUnit
                        }
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }

    public async Task AddAsync(Scene scene)
    {
        if (scene.Id == Guid.Empty)
        {
            scene.Id = Guid.NewGuid();
        }

        var entity = new SceneEntity
        {
            Id = scene.Id,
            Name = scene.Name,
            IsPublic = scene.IsPublic,
            Actions = scene.Actions.Select(a => new SceneActionEntity
            {
                Id = Guid.NewGuid(),
                DeviceId = a.DeviceId,
                TargetValue = a.TargetState.Value,
                TargetUnit = a.TargetState.Unit
            }).ToList()
        };

        _db.Scenes.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Scene scene)
    {
        var entity = await _db.Scenes
            .Include(s => s.Actions)
            .SingleOrDefaultAsync(s => s.Id == scene.Id);

        if (entity is null)
        {
            return;
        }

        entity.Name = scene.Name;
        entity.IsPublic = scene.IsPublic;

        _db.SceneActions.RemoveRange(entity.Actions);

        entity.Actions = scene.Actions.Select(a => new SceneActionEntity
        {
            Id = Guid.NewGuid(),
            SceneId = scene.Id,
            DeviceId = a.DeviceId,
            TargetValue = a.TargetState.Value,
            TargetUnit = a.TargetState.Unit
        }).ToList();

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Scenes.SingleOrDefaultAsync(s => s.Id == id);
        if (entity is null)
        {
            return;
        }

        _db.Scenes.Remove(entity);
        await _db.SaveChangesAsync();
    }
}