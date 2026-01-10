using System;
using System.Linq;
using System.Threading.Tasks;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IO_projekt_symulator.Server.Contracts;

namespace IO_Panel.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SceneController : ControllerBase
    {
        private readonly ISceneRepository _sceneRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<SceneController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public SceneController(ISceneRepository sceneRepository, IDeviceRepository deviceRepository, ILogger<SceneController> logger, IPublishEndpoint publishEndpoint)
        {
            _sceneRepository = sceneRepository;
            _deviceRepository = deviceRepository;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public sealed record CreateSceneRequest(string Name, bool IsPublic, SceneActionDto[] Actions);
        public sealed record SceneActionDto(string DeviceId, DeviceState TargetState);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var scenes = await _sceneRepository.GetAllAsync();
            return Ok(scenes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var scene = await _sceneRepository.GetByIdAsync(id);
            if (scene == null)
            {
                return NotFound();
            }
            return Ok(scene);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSceneRequest request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Scene name is required.");
            }

            var scene = new Scene
            {
                Name = request.Name.Trim(),
                IsPublic = request.IsPublic,
                Actions = request.Actions?.Select(a => new SceneAction
                {
                    DeviceId = a.DeviceId,
                    TargetState = a.TargetState
                }).ToList() ?? new()
            };

            await _sceneRepository.AddAsync(scene);
            return CreatedAtAction(nameof(GetById), new { id = scene.Id }, scene);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Scene scene)
        {
            if (scene == null || scene.Id != id)
            {
                return BadRequest();
            }

            var existingScene = await _sceneRepository.GetByIdAsync(id);
            if (existingScene == null)
            {
                return NotFound();
            }

            await _sceneRepository.UpdateAsync(scene);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingScene = await _sceneRepository.GetByIdAsync(id);
            if (existingScene == null)
            {
                return NotFound();
            }

            await _sceneRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateScene(Guid id)
        {
            var scene = await _sceneRepository.GetByIdAsync(id);
            if (scene == null)
            {
                return NotFound();
            }

            if (!scene.IsPublic && !(User?.Identity?.IsAuthenticated ?? false))
            {
                return Unauthorized();
            }

            foreach (var action in scene.Actions)
            {
                if (Guid.TryParse(action.DeviceId, out var deviceIdGuid))
                {
                    var command = new SetDeviceStateCommand
                    {
                        DeviceId = deviceIdGuid,
                        Value = action.TargetState.Value,
                        Unit = action.TargetState.Unit
                    };

                    await _publishEndpoint.Publish(command);
                }
                else
                {
                    _logger.LogWarning("Invalid Device GUID in scene action: {DeviceId}", action.DeviceId);
                }
            }

            return Accepted();
        }
    }
}