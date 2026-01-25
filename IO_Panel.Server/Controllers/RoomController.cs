using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IO_Panel.Server.Controllers
{
    /// <summary>
    /// CRUD API for rooms and room membership management (assigning/removing devices from rooms).
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _roomRepo;

        public RoomController(IRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        /// <summary>
        /// Returns all rooms.
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<Room>> GetAll()
        {
            return await _roomRepo.GetAllAsync();
        }

        /// <summary>
        /// Request payload for room creation.
        /// </summary>
        public sealed record CreateRoomRequest(string Name);

        /// <summary>
        /// Admin-only. Creates a room with a validated, trimmed name.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomRequest request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Room name is required.");
            }

            var room = new Room
            {
                Name = request.Name.Trim(),
                DeviceIds = new()
            };

            await _roomRepo.AddAsync(room);

            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }

        /// <summary>
        /// Returns a room by id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetById(Guid id)
        {
            var room = await _roomRepo.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            return Ok(room);
        }

        /// <summary>
        /// Returns current device snapshots for a room.
        /// </summary>
        [HttpGet("{roomId}/devices")]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices(Guid roomId)
        {
            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room == null)
            {
                return NotFound("Room not found");
            }

            var devices = await _roomRepo.GetDevicesInRoomAsync(roomId);
            return Ok(devices);
        }

        /// <summary>
        /// Admin-only. Adds a device to a room.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{roomId}/devices/{deviceId}")]
        public async Task<IActionResult> AddDevice(Guid roomId, string deviceId)
        {
            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room == null)
            {
                return NotFound("Room not found");
            }

            await _roomRepo.AddDeviceToRoomAsync(roomId, deviceId);

            return Ok();
        }

        /// <summary>
        /// Admin-only. Removes a device from a room.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{roomId}/devices/{deviceId}")]
        public async Task<IActionResult> RemoveDevice(Guid roomId, string deviceId)
        {
            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room == null)
            {
                return NotFound("Room not found");
            }

            await _roomRepo.RemoveDeviceFromRoomAsync(roomId, deviceId);
            return NoContent();
        }

        /// <summary>
        /// Admin-only. Deletes a room.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var room = await _roomRepo.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            await _roomRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}