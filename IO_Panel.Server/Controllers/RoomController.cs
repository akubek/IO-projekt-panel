using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IO_Panel.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _roomRepo;

        public RoomController(IRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        [HttpGet]
        public async Task<IEnumerable<Room>> GetAll()
        {
            return await _roomRepo.GetAllAsync();
        }

        public sealed record CreateRoomRequest(string Name);

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

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetById(Guid id)
        {
            var room = await _roomRepo.GetByIdAsync(id);
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpGet("{roomId}/devices")]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices(Guid roomId)
        {
            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room == null) return NotFound("Room not found");

            var devices = await _roomRepo.GetDevicesInRoomAsync(roomId);
            return Ok(devices);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{roomId}/devices/{deviceId}")]
        public async Task<IActionResult> AddDevice(Guid roomId, string deviceId)
        {
            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room == null) return NotFound("Room not found");

            await _roomRepo.AddDeviceToRoomAsync(roomId, deviceId);

            return Ok();
        }

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