using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories.Entities;

namespace IO_Panel.Server.Mappers
{
    public static class DeviceMapper
    {
        // map API model -> domain model
        public static Device ToDomain(this ApiDevice api, string? id = null, DateTime? lastSeen = null)
        {
            return new Device
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Name = api.Name ?? string.Empty,
                Type = api.Type ?? string.Empty,
                Location = api.Location ?? string.Empty,
                Description = api.Description ?? string.Empty,
                State = new DeviceState
                {
                    Value = api.State?.Value ?? 0,
                    Unit = api.State?.Unit
                },
                Config = new DeviceConfig
                {
                    // ApiDeviceConfig exposes "Readonly" while domain uses "ReadOnly"
                    ReadOnly = api.Config?.Readonly ?? false,
                    Min = api.Config?.Min ?? 0,
                    Max = api.Config?.Max ?? 0,
                    Step = api.Config?.Step ?? 0
                },
                LastSeen = lastSeen ?? DateTime.UtcNow,
                Status = "Unknown",
                CreatedAt = api.CreatedAt
            };
        }

        // optional: map domain -> API model (if you need to send config back)
        public static ApiDevice ToApi(this Device d)
        {
            return new ApiDevice
            {
                Name = d.Name,
                Type = d.Type,
                Location = d.Location,
                Description = d.Description,
                State = new ApiDeviceState { Value = d.State.Value, Unit = d.State.Unit },
                Config = new ApiDeviceConfig { Readonly = d.Config.ReadOnly, Min = d.Config.Min, Max = d.Config.Max, Step = d.Config.Step },
                CreatedAt = d.CreatedAt
            };
        }
    }
}