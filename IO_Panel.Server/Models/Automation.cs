using System;

namespace IO_Panel.Server.Models
{
    public sealed class Automation
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }

        public AutomationTrigger Trigger { get; set; } = new();
        public AutomationAction Action { get; set; } = new();
    }

    public sealed class AutomationTrigger
    {
        public AutomationCondition[] Conditions { get; set; } = Array.Empty<AutomationCondition>();
        public TimeOfDayWindow? TimeWindow { get; set; }
    }

    public sealed class AutomationCondition
    {
        public string DeviceId { get; set; } = string.Empty;
        public AutomationComparisonOperator Operator { get; set; }
        public double Value { get; set; }
        public string? Unit { get; set; }
    }

    public enum AutomationComparisonOperator
    {
        Equal = 0,
        GreaterThan = 1,
        GreaterThanOrEqual = 2,
        LessThan = 3,
        LessThanOrEqual = 4
    }

    public sealed class TimeOfDayWindow
    {
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
    }

    public sealed class AutomationAction
    {
        public AutomationActionKind Kind { get; set; }

        public string? DeviceId { get; set; }
        public double? TargetValue { get; set; }
        public string? TargetUnit { get; set; }

        public Guid? SceneId { get; set; }
    }

    public enum AutomationActionKind
    {
        SetDeviceState = 0,
        RunScene = 1
    }
}