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
using MassTransit;
using IO_Panel.Server.Contracts;

namespace IO_Panel.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly IDeviceRepository _repo;
        private readonly IDeviceApiClient _apiClient;
        private readonly IPublishEndpoint _publishEndpoint;

        public DeviceController(ILogger<DeviceController> logger, IDeviceRepository repo, IDeviceApiClient apiClient, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _repo = repo;
            _apiClient = apiClient;
            _publishEndpoint = publishEndpoint;
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
            if (!Guid.TryParse(id, out var deviceIdGuid))
            {
                return BadRequest("Device ID must be a valid GUID.");
            }

            // Create the command object based on the shared contract
            var command = new SetDeviceStateCommand
            {
                DeviceId = deviceIdGuid,
                Value = state.Value,
                Unit = state.Unit
            };

            // Publish the command to the message bus
            await _publishEndpoint.Publish(command, ct);

            _logger.LogInformation("Published SetDeviceStateCommand for DeviceId {DeviceId}", id);

            // Return 'Accepted' to indicate the command has been queued for processing
            return Accepted(new { deviceId = id, state });
        }

        public record CommandDto
        {
            public string Command { get; init; } = default!;
            public string? Payload { get; init; }
        }
    }
}
