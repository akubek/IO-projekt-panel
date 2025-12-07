using System;
using System.Threading.Tasks;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IO_Panel.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SceneController : ControllerBase
    {
        private readonly ISceneRepository _sceneRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<SceneController> _logger;

        public SceneController(ISceneRepository sceneRepository, IDeviceRepository deviceRepository, ILogger<SceneController> logger)
        {
            _sceneRepository = sceneRepository;
            _deviceRepository = deviceRepository;
            _logger = logger;
        }

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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Scene scene)
        {
            if (scene == null)
            {
                return BadRequest();
            }
            await _sceneRepository.AddAsync(scene);
            return CreatedAtAction(nameof(GetById), new { id = scene.Id }, scene);
        }

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

            foreach (var action in scene.Actions)
            {
                try
                {
                    await _deviceRepository.RequestStateChangeAsync(action.DeviceId, action.TargetState, HttpContext.RequestAborted);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to apply scene action for device {DeviceId}", action.DeviceId);
                    // Continue to next action even if one fails
                }
            }

            return Accepted();
        }
    }
}