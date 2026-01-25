namespace IO_Panel.Server.Data.Entities;

/// <summary>
/// Persisted action within a scene: set a target value/unit on a specific device.
/// </summary>
public sealed class SceneActionEntity
{
    public Guid Id { get; set; }

    public Guid SceneId { get; set; }
    public SceneEntity Scene { get; set; } = default!;

    public string DeviceId { get; set; } = string.Empty;

    public double TargetValue { get; set; }
    public string? TargetUnit { get; set; }
}