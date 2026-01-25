using System;

namespace IO_Panel.Server.Models
{
    /// <summary>
    /// User-defined rule combining a trigger (conditions/time window) and an action (set device state or run scene).
    /// </summary>
    public sealed class Automation
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }

        public AutomationTrigger Trigger { get; set; } = new();
        public AutomationAction Action { get; set; } = new();
    }

    /// <summary>
    /// Trigger definition: device conditions plus optional time-of-day window.
    /// </summary>
    public sealed class AutomationTrigger
    {
        public AutomationCondition[] Conditions { get; set; } = Array.Empty<AutomationCondition>();
        public TimeOfDayWindow? TimeWindow { get; set; }
    }

    /// <summary>
    /// Single condition comparing a device value with a threshold using an operator.
    /// </summary>
    public sealed class AutomationCondition
    {
        public string DeviceId { get; set; } = string.Empty;
        public AutomationComparisonOperator Operator { get; set; }
        public double Value { get; set; }
        public string? Unit { get; set; }
    }

    /// <summary>
    /// Comparison operator used by automation conditions.
    /// </summary>
    public enum AutomationComparisonOperator
    {
        Equal = 0,
        GreaterThan = 1,
        GreaterThanOrEqual = 2,
        LessThan = 3,
        LessThanOrEqual = 4
    }

    /// <summary>
    /// Local time window in which an automation is allowed to run.
    /// </summary>
    public sealed class TimeOfDayWindow
    {
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
    }

    /// <summary>
    /// Action executed when trigger is satisfied (device state write or scene activation).
    /// </summary>
    public sealed class AutomationAction
    {
        public AutomationActionKind Kind { get; set; }

        public string? DeviceId { get; set; }
        public double? TargetValue { get; set; }
        public string? TargetUnit { get; set; }

        public Guid? SceneId { get; set; }
    }

    /// <summary>
    /// Supported automation action kinds.
    /// </summary>
    public enum AutomationActionKind
    {
        SetDeviceState = 0,
        RunScene = 1
    }
}