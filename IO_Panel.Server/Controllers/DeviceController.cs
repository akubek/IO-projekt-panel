using IO_projekt_symulator.Server.Contracts;
using IO_Panel.Server.Mappers;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IO_Panel.Server.Controllers
{
    /// <summary>
    /// REST API for configured devices: list/read/update metadata, publish state commands, and query state history.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly IDeviceRepository _repo;
        private readonly IDeviceApiClient _apiClient;
        private readonly IPublishEndpoint _publishEndpoint;

        public DeviceController(
            ILogger<DeviceController> logger,
            IDeviceRepository repo,
            IDeviceApiClient apiClient,
            IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _repo = repo;
            _apiClient = apiClient;
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// Returns configured devices from local storage.
        /// If the external simulator API is unavailable, devices are returned as Offline/Malfunctioning (best-effort UI behavior).
        /// </summary>
        [HttpGet(Name = "GetDevices")]
        public async Task<ActionResult<IEnumerable<Device>>> Get(CancellationToken ct)
        {
            var devices = await _repo.GetConfiguredDevicesAsync(ct);

            try
            {
                return Ok(devices);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Device API is unavailable. Returning DB devices as Offline/Malfunctioning.");

                foreach (var device in devices)
                {
                    device.Status = "Offline";
                    device.Malfunctioning = true;
                }

                return Ok(devices);
            }
        }

        /// <summary>
        /// Fetches raw devices from the external simulator API (debug/admin use).
        /// </summary>
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

        /// <summary>
        /// Returns a single configured device by id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> Get(string id, CancellationToken ct)
        {
            var device = await _repo.GetByIdAsync(id, ct);
            if (device is null)
            {
                return NotFound();
            }

            return Ok(device);
        }

        /// <summary>
        /// Creates a configured device in local storage by importing an external simulator device and assigning a display name.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Device>> ConfigureDevice([FromBody] ConfigureDeviceRequestDto request, CancellationToken ct)
        {
            if (request is null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(request.ApiDeviceId))
            {
                return BadRequest("Device ID is required.");
            }

            var apiDevice = await _apiClient.GetByIdAsync(request.ApiDeviceId, ct);
            if (apiDevice is null)
            {
                return NotFound("The device to be configured was not found in the external API.");
            }

            var device = apiDevice.ToDomain(name: request.DisplayName, lastSeen: DateTime.UtcNow);
            device.ConfiguredAt = DateTimeOffset.UtcNow;

            await _repo.AddAsync(device, ct);

            return CreatedAtAction(nameof(Get), new { id = device.Id }, device);
        }

        /// <summary>
        /// Updates user-facing metadata for a configured device (display name/localization).
        /// </summary>
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

        /// <summary>
        /// Placeholder command endpoint (currently accepts the payload and returns Accepted without publishing).
        /// </summary>
        [HttpPost("{id}/command")]
        public ActionResult SendCommand(string id, [FromBody] CommandDto cmd)
        {
            return Accepted(new { deviceId = id, command = cmd.Command, status = "queued" });
        }

        /// <summary>
        /// Publishes a SetDeviceStateCommand to RabbitMQ for asynchronous execution by the simulator.
        /// </summary>
        [HttpPost("{id}/state")]
        public async Task<ActionResult> SetState(string id, [FromBody] DeviceState state, CancellationToken ct)
        {
            // Device IDs are GUID strings in this system; reject non-GUID values early.
            if (!Guid.TryParse(id, out var deviceIdGuid))
            {
                return BadRequest("Device ID must be a valid GUID.");
            }

            var command = new SetDeviceStateCommand
            {
                DeviceId = deviceIdGuid,
                Value = state.Value,
                Unit = state.Unit
            };

            await _publishEndpoint.Publish(command, ct);

            _logger.LogInformation("Published SetDeviceStateCommand for DeviceId {DeviceId}", id);

            return Accepted(new { deviceId = id, state });
        }

        /// <summary>
        /// Admin-only. Returns external simulator devices that are not yet configured in the panel database.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/unconfigured")]
        public async Task<ActionResult<IEnumerable<ApiDevice>>> GetUnconfiguredDevices(CancellationToken ct)
        {
            var devices = await _repo.GetUnconfiguredDevicesAsync(ct);
            return Ok(devices);
        }

        /// <summary>
        /// Admin-only. Deletes a configured device from local storage.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<ActionResult> DeleteConfiguredDevice(string id, CancellationToken ct)
        {
            var device = await _repo.GetByIdAsync(id, ct);
            if (device is null)
            {
                return NotFound();
            }

            await _repo.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Returns device state history points in a time range (defaults to last 24h).
        /// </summary>
        [HttpGet("{id}/history")]
        public async Task<ActionResult> GetHistory(
            string id,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] int limit = 200,
            CancellationToken ct = default)
        {
            var toValue = to ?? DateTimeOffset.UtcNow;
            var fromValue = from ?? toValue.AddHours(-24);

            if (fromValue > toValue)
            {
                return BadRequest("'from' must be <= 'to'.");
            }

            var history = await _repo.GetDeviceHistoryAsync(id, fromValue, toValue, limit, ct);
            return Ok(history);
        }

        /// <summary>
        /// Generic command payload (currently only used by the placeholder command endpoint).
        /// </summary>
        public record CommandDto
        {
            public string Command { get; init; } = default!;
            public string? Payload { get; init; }
        }

        /// <summary>
        /// Update payload for configured device UI metadata.
        /// </summary>
        public record DeviceUpdateDto(string DisplayName, string? Localization);

        /// <summary>
        /// Request payload to configure (import) an external simulator device into the panel DB.
        /// </summary>
        public record ConfigureDeviceRequestDto(string ApiDeviceId, string DisplayName);
    }
}
