using System.Text.Json.Serialization;

namespace IO_Panel.Server.Models
{
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

    public class ApiDeviceState
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }
    }

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