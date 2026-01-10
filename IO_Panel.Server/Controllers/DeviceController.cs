using IO_projekt_symulator.Server.Contracts;
using IO_Panel.Server.Mappers;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IO_Panel.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly IDeviceRepository _repo;
        private readonly IDeviceApiClient _apiClient;
        private readonly IPublishEndpoint _publishEndpoint; // RabbitMQ publish endpoint

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
            try
            {
                var devices = await _repo.GetConfiguredDevicesAsync(ct);
                return Ok(devices);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to load devices from external API.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Device API is unavailable.");
            }
        }

        [HttpGet("external")]
        public async Task<ActionResult<IEnumerable<ApiDevice>>> GetExternal(CancellationToken ct)
        {
            try
            {
                var list = await _apiClient.GetAllAsync(ct);
                return Ok(list);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to load external devices from external API.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Device API is unavailable.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> Get(string id, CancellationToken ct)
        {
            var device = await _repo.GetByIdAsync(id, ct);
            if (device is null) return NotFound();
            return Ok(device);
        }

        // Configure a new device
        [HttpPost]
        public async Task<ActionResult<Device>> ConfigureDevice([FromBody] ConfigureDeviceRequestDto request, CancellationToken ct)
        {
            if (request is null) return BadRequest();
            if (string.IsNullOrWhiteSpace(request.ApiDeviceId)) return BadRequest("Device ID is required.");

            var apiDevice = await _apiClient.GetByIdAsync(request.ApiDeviceId, ct);
            if (apiDevice is null)
            {
                return NotFound("The device to be configured was not found in the external API.");
            }

            Device device = apiDevice.ToDomain(name: request.DisplayName, lastSeen: DateTime.UtcNow);

            await _repo.AddAsync(device, ct);

            return CreatedAtAction(nameof(Get), new { id = device.Id }, device);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(string id, [FromBody] DeviceUpdateDto updateDto, CancellationToken ct)
        {
            var device = await _repo.GetByIdAsync(id, ct);
            if (device is null)
            {
                return NotFound();
            }

            device.DisplayName = updateDto.DisplayName;
            device.Localization = updateDto.Localization;
            device.LastSeen = DateTime.UtcNow;

            await _repo.UpdateAsync(device, ct);

            return NoContent();
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

        // Admin endpoint to retrieve external unconfigured devices with detailed information
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/unconfigured")]
        public async Task<ActionResult<IEnumerable<ApiDevice>>> GetUnconfiguredDevices(CancellationToken ct)
        {
            var devices = await _repo.GetUnconfiguredDevicesAsync(ct);
            return Ok(devices);
        }

        // Admin endpoint to delete a device
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<ActionResult> DeleteConfiguredDevice(string id, CancellationToken ct)
        {
            var device = await _repo.GetByIdAsync(id, ct);
            if (device is null) return NotFound();

            await _repo.DeleteAsync(id, ct);
            return NoContent();
        }

        public record CommandDto
        {
            public string Command { get; init; } = default!;
            public string? Payload { get; init; }
        }

        public record DeviceUpdateDto(string DisplayName, string? Localization);

        public record ConfigureDeviceRequestDto(string ApiDeviceId, string DisplayName);
    }
}
