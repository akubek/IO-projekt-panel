using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

//Próba stworzenia podstawowego kontrolera backendu do zarządzania urządzeniami IoT, aktualnie nie przyjmują żadnych komend, jedynie zwracana jest lista urządzeń przy GET
//przy uruchamianiu strony oraz pojedyncze urządzenie przy GET /{id}
namespace IO_Panel.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        //logger do logowania informacji, copilot dodał
        private readonly ILogger<DeviceController> _logger;
        private readonly IDeviceRepository _repo;

        public DeviceController(ILogger<DeviceController> logger, IDeviceRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        //metoda do pobierania listy urządzeń na endpoint /device, czyli uruchamiana automatycznie przy GET (początek uruchomienia strony)
        [HttpGet(Name = "GetDevices")]
        public async Task<ActionResult<IEnumerable<Device>>> Get(CancellationToken ct)
        {
            var devices = await _repo.GetAllAsync(ct);
            return Ok(devices);
        }

        //metoda do pobierania pojedynczego urządzenia na endpoint /device/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> Get(string id, CancellationToken ct)
        {
            var device = await _repo.GetByIdAsync(id, ct);
            if (device is null) return NotFound();
            return Ok(device);
        }

        //metoda do wysyłania komend do urządzenia na endpoint /device/{id}/command
        [HttpPost("{id}/command")]
        public ActionResult SendCommand(string id, [FromBody] CommandDto cmd)
        {
            // Tutaj komendy do urządzeń, np włącz wyłącz itp.
            // Na razie zwracamy tylko symulację przyjęcia
            return Accepted(new { deviceId = id, command = cmd.Command, status = "queued" }); //copilot dodał
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

        public record CommandDto //Struktura komendy do urządzenia
        {
            public string Command { get; init; } = default!;
            public string? Payload { get; init; } //Nie wiem co to, copilot to dodał
        }
    }
}
