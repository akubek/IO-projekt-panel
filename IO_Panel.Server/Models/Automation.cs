using System;

namespace IO_Panel.Server.Models
{
    public class Automation
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }

        // TODO: Define the structure for triggers and actions.
        public string LogicDefinition { get; set; } = string.Empty;
    }
}