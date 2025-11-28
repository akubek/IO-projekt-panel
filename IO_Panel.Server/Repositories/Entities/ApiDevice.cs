using System.Text.Json.Serialization;

namespace IO_Panel.Server.Repositories.Entities
{
    public class ApiDevice
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public ApiDeviceState? State { get; set; }
        public ApiDeviceConfig? Config { get; set; }
    }

    public class ApiDeviceState
    {
        public double Value { get; set; }
        public string? Unit { get; set; }
    }

    public class ApiDeviceConfig
    {
        [JsonPropertyName("readonly")]
        public bool ReadOnly { get; set; }

        public double Min { get; set; }
        public double Max { get; set; }
        public double Step { get; set; }
    }
}