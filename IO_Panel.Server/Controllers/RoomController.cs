using System;
using System.Threading.Tasks;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IO_Panel.Server.Controllers
{
    [ApiController]
    [Route("[controller]")] // lub Route("api/rooms")
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _roomRepo;
        // Opcjonalnie wstrzyknij RoomService, jeśli logika jest złożona

        public RoomController(IRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        [HttpGet]
        public async Task<IEnumerable<Room>> GetAll()
        {
            return await _roomRepo.GetAllAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Room room)
        {
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

        // Endpoint do dodawania urządzenia do pokoju (REST-owo)
        [HttpPost("{roomId}/devices/{deviceId}")]
        public async Task<IActionResult> AddDevice(Guid roomId, string deviceId)
        {
            // Tutaj logika: pobierz pokój, dodaj ID do listy DeviceIds, zapisz
            // Najlepiej wywołać metodę z serwisu np. _roomService.AddDeviceToRoomAsync(roomId, deviceId)

            var room = await _roomRepo.GetByIdAsync(roomId);
            if (room == null) return NotFound("Room not found");

            if (!room.DeviceIds.Contains(deviceId))
            {
                room.DeviceIds.Add(deviceId);
                await _roomRepo.UpdateAsync(room);
            }

            return Ok();
        }
    }
}