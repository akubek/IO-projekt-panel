using System;
using System.Collections.Generic;

namespace IO_Panel.Server.Models
{
    public class Scene
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public List<SceneAction> Actions { get; set; } = new();
    }

    public class SceneAction
    {
        public string DeviceId { get; set; } = string.Empty;
        public DeviceState TargetState { get; set; } = new();
    }
}