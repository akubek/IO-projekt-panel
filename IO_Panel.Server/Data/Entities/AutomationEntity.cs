namespace IO_Panel.Server.Data.Entities;

/// <summary>
/// Persisted automation definition. Trigger and action are stored as JSON for flexibility/evolution.
/// </summary>
public sealed class AutomationEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }

    /// <summary>
    /// JSON-serialized <c>AutomationTrigger</c>.
    /// </summary>
    public string TriggerJson { get; set; } = "{}";

    /// <summary>
    /// JSON-serialized <c>AutomationAction</c>.
    /// </summary>
    public string ActionJson { get; set; } = "{}";
}