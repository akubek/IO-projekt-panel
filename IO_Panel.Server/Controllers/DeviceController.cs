using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using IO_Panel.Server.Repositories.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IO_Panel.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly IDeviceRepository _repo;
        private readonly IDeviceApiClient _apiClient;

        public DeviceController(ILogger<DeviceController> logger, IDeviceRepository repo, IDeviceApiClient apiClient)
        {
            _logger = logger;
            _repo = repo;
            _apiClient = apiClient;
        }

        [HttpGet(Name = "GetDevices")]
        public async Task<ActionResult<IEnumerable<Device>>> Get(CancellationToken ct)
        {
            var devices = await _repo.GetAllAsync(ct);
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> Get(string id, CancellationToken ct)
        {
            var device = await _repo.GetByIdAsync(id, ct);
            if (device is null) return NotFound();
            return Ok(device);
        }

        // Return devices from the external API
        [HttpGet("external")]
        public async Task<ActionResult<IEnumerable<ApiDevice>>> GetExternal(CancellationToken ct)
        {
            var list = await _apiClient.GetAllAsync(ct);
            return Ok(list);
        }

        // Create / add configured device
        [HttpPost]
        public async Task<ActionResult<Device>> Create([FromBody] Device device, CancellationToken ct)
        {
            if (device is null) return BadRequest();
            if (string.IsNullOrWhiteSpace(device.Id)) device.Id = Guid.NewGuid().ToString();

            await _repo.AddAsync(device, ct);

            return CreatedAtAction(nameof(Get), new { id = device.Id }, device);
        }

        [HttpPost("{id}/command")]
        public ActionResult SendCommand(string id, [FromBody] CommandDto cmd)
        {
            return Accepted(new { deviceId = id, command = cmd.Command, status = "queued" });
        }

        [HttpPost("{id}/state")]
        public async Task<ActionResult> SetState(string id, [FromBody] DeviceState state, CancellationToken ct)
        {
            try
            {
                await _repo.RequestStateChangeAsync(id, state, ct);
                return Accepted(new { deviceId = id, state });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        public record CommandDto
        {
            public string Command { get; init; } = default!;
            public string? Payload { get; init; }
        }
    }
}
