using IO_Panel.Server.Models;

namespace IO_Panel.Server.Mappers
{
    /// <summary>
    /// Mapping helpers between external simulator DTOs (<see cref="ApiDevice"/>) and internal domain models (<see cref="Device"/>).
    /// </summary>
    public static class DeviceMapper
    {
        /// <summary>
        /// Converts an external simulator device DTO into the panel domain model, applying UI defaults.
        /// </summary>
        public static Device ToDomain(this ApiDevice api, string? name = null, DateTime? lastSeen = null)
        {
            if (api.Id == null)
            {
                throw new ArgumentNullException(nameof(api.Id), "API Device ID cannot be null.");
            }

            return new Device
            {
                Id = api.Id,
                DeviceName = api.Name ?? string.Empty,
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
                    // ApiDeviceConfig exposes "Readonly" while domain uses "ReadOnly".
                    ReadOnly = api.Config?.Readonly ?? false,
                    Min = api.Config?.Min ?? 0,
                    Max = api.Config?.Max ?? 0,
                    Step = api.Config?.Step ?? 0
                },
                DisplayName = name ?? api.Name ?? string.Empty,
                LastSeen = lastSeen ?? DateTime.UtcNow,
                Status = "Online",
                CreatedAt = api.CreatedAt
            };
        }

        /// <summary>
        /// Converts the panel domain model into an external simulator DTO (used when calling simulator endpoints).
        /// </summary>
        public static ApiDevice ToApi(this Device d)
        {
            return new ApiDevice
            {
                Name = d.DeviceName,
                Type = d.Type,
                Location = d.Location,
                Description = d.Description,
                State = new ApiDeviceState { Value = d.State.Value, Unit = d.State.Unit },
                Config = new ApiDeviceConfig
                {
                    Readonly = d.Config.ReadOnly,
                    Min = d.Config.Min,
                    Max = d.Config.Max,
                    Step = d.Config.Step
                },
                CreatedAt = d.CreatedAt
            };
        }

        /// <summary>
        /// Updates an existing <see cref="Device"/> instance with values from <see cref="ApiDevice"/>.
        /// </summary>
        /// <remarks>
        /// This method currently reassigns the local parameter (<c>d = ...</c>), which does not modify the caller's instance.
        /// If it's intended to mutate, copy properties onto <paramref name="d"/> instead of reassigning.
        /// </remarks>
        internal static void UpdateFromApiDevice(this Device d, ApiDevice apiDevice)
        {
            if (d.Id != apiDevice.Id)
            {
                throw new ArgumentException("Device ID mismatch between domain and API models.");
            }

            d = apiDevice.ToDomain(name: d.DeviceName);
        }
    }
}