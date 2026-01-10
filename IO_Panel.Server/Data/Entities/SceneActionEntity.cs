namespace IO_Panel.Server.Data.Entities;

public sealed class SceneActionEntity
{
    public Guid Id { get; set; }

    public Guid SceneId { get; set; }
    public SceneEntity Scene { get; set; } = default!;

    public string DeviceId { get; set; } = string.Empty;

    public double TargetValue { get; set; }
    public string? TargetUnit { get; set; }
}