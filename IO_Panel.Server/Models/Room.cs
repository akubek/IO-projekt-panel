using System;
using System.Collections.Generic;

namespace IO_Panel.Server.Models
{
    public class Room
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> DeviceIds { get; set; } = new();
    }
}