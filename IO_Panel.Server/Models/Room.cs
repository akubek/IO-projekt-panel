using System;
using System.Collections.Generic;

namespace IO_Panel.Server.Models
{
    /// <summary>
    /// Domain model for a room grouping configured devices by id.
    /// </summary>
    public class Room
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// List of configured device ids assigned to this room.
        /// </summary>
        public List<string> DeviceIds { get; set; } = new();
    }
}