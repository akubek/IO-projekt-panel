using System;
using System.Collections.Generic;

namespace IO_Panel.Server.Models
{
    /// <summary>
    /// A predefined set of device state changes that can be activated as a single operation.
    /// </summary>
    public class Scene
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// If true, the scene can be activated without authentication.
        /// </summary>
        public bool IsPublic { get; set; }

        public List<SceneAction> Actions { get; set; } = new();
    }

    /// <summary>
    /// One device state change within a scene.
    /// </summary>
    public class SceneAction
    {
        public string DeviceId { get; set; } = string.Empty;
        public DeviceState TargetState { get; set; } = new();
    }
}