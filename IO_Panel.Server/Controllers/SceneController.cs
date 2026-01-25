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
    /// <summary>
    /// CRUD API for scenes. Activating a scene publishes a batch of device state commands to RabbitMQ.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SceneController : ControllerBase
    {
        private readonly ISceneRepository _sceneRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<SceneController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public SceneController(
            ISceneRepository sceneRepository,
            IDeviceRepository deviceRepository,
            ILogger<SceneController> logger,
            IPublishEndpoint publishEndpoint)
        {
            _sceneRepository = sceneRepository;
            _deviceRepository = deviceRepository;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Request payload for scene creation.
        /// </summary>
        public sealed record CreateSceneRequest(string Name, bool IsPublic, SceneActionDto[] Actions);

        /// <summary>
        /// Request payload describing a single scene action.
        /// </summary>
        public sealed record SceneActionDto(string DeviceId, DeviceState TargetState);

        /// <summary>
        /// Returns all scenes.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var scenes = await _sceneRepository.GetAllAsync();
            return Ok(scenes);
        }

        /// <summary>
        /// Returns a scene by id.
        /// </summary>
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

        /// <summary>
        /// Admin-only. Creates a scene definition.
        /// </summary>
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

        /// <summary>
        /// Admin-only. Updates a scene definition.
        /// </summary>
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

        /// <summary>
        /// Admin-only. Deletes a scene definition.
        /// </summary>
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

        /// <summary>
        /// Activates a scene by publishing a SetDeviceStateCommand for each configured scene action.
        /// Public scenes can be activated without authentication; private scenes require authentication.
        /// </summary>
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