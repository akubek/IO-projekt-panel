using System.Text.Json.Serialization;

namespace IO_Panel.Server.Models
{
    /// <summary>
    /// DTO representing a device returned by the external simulator API.
    /// Keep in sync with the simulator contract.
    /// </summary>
    public class ApiDevice
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public ApiDeviceState? State { get; set; }

        [JsonPropertyName("config")]
        public ApiDeviceConfig? Config { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonPropertyName("malfunctioning")]
        public bool Malfunctioning { get; set; }
    }

    /// <summary>
    /// Device state payload returned by the simulator API.
    /// </summary>
    public class ApiDeviceState
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }
    }

    /// <summary>
    /// Device configuration payload returned by the simulator API.
    /// </summary>
    public class ApiDeviceConfig
    {
        [JsonPropertyName("min")]
        public double? Min { get; set; }

        [JsonPropertyName("max")]
        public double? Max { get; set; }

        [JsonPropertyName("step")]
        public double? Step { get; set; }

        [JsonPropertyName("readonly")]
        public bool Readonly { get; set; }
    }
}