using Microsoft.AspNetCore.Mvc;

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
        public DeviceController(ILogger<DeviceController> logger)
        {
            _logger = logger;
        }

        // Przykładowe dane urządzeń, aktualnie stałe
        private static readonly List<DeviceDto> Devices = new()
        {
            new DeviceDto { Id = "dev-1", Name = "Sensor A", Type = "Sensor", Status = "Online", LastSeen = DateTime.UtcNow.AddMinutes(-1), Localization = "Living room" },
            new DeviceDto { Id = "dev-2", Name = "Lamp B",  Type = "Switch", Status = "Offline", LastSeen = DateTime.UtcNow.AddHours(-1), Localization = "Kitchen" },
            new DeviceDto { Id = "dev-3", Name = "Thermometer C",  Type = "Slider", Status = "Online", LastSeen = DateTime.UtcNow.AddMinutes(-1), Localization = "Garage" }
        };

        //metoda do pobierania listy urządzeń na endpoint /device, czyli uruchamiana automatycznie przy GET (początek uruchomienia strony)
        [HttpGet(Name = "GetDevice")]
        public ActionResult<IEnumerable<DeviceDto>> Get() => Ok(Devices);

        //metoda do pobierania pojedynczego urządzenia na endpoint /device/{id}
        [HttpGet("{id}")]
        public ActionResult<DeviceDto> Get(string id)
        {
            var d = Devices.FirstOrDefault(dev => dev.Id == id);
            if (d == null)
            {
                return NotFound();
            }
            return Ok(d);
        }

        //metoda do wysyłania komend do urządzenia na endpoint /device/{id}/command
        [HttpPost("{id}/command")]
        public ActionResult SendCommand(string id, [FromBody] CommandDto cmd)
        {
            // Tutaj komendy do urządzeń, np włącz wyłącz itp.
            // Na razie zwracamy tylko symulację przyjęcia
            return Accepted(new { deviceId = id, command = cmd.Command, status = "queued" }); //copilot dodał
        }
        public record DeviceDto //Struktura urządzenia
        {
            public string Id { get; init; } = default!;
            public string Name { get; init; } = default!;
            public string Type { get; init; } = default!;
            public string Status { get; init; } = default!;
            public DateTime LastSeen { get; init; }
            public string Localization { get; init; } = default!;
        }

        public record CommandDto //Struktura komendy do urządzenia
        {
            public string Command { get; init; } = default!;
            public string? Payload { get; init; } //Nie wiem co to, copilot to dodał
        }
    }
}
