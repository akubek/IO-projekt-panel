using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories.InMemory
{
    public class SceneRepository : ISceneRepository
    {
        private readonly ConcurrentDictionary<Guid, Scene> _scenes = new();

        public Task<IEnumerable<Scene>> GetAllAsync()
        {
            return Task.FromResult(_scenes.Values.AsEnumerable());
        }

        public Task<Scene?> GetByIdAsync(Guid id)
        {
            _scenes.TryGetValue(id, out var scene);
            return Task.FromResult(scene);
        }

        public Task AddAsync(Scene scene)
        {
            scene.Id = Guid.NewGuid();
            _scenes[scene.Id] = scene;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Scene scene)
        {
            _scenes[scene.Id] = scene;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _scenes.TryRemove(id, out _);
            return Task.CompletedTask;
        }
    }
}