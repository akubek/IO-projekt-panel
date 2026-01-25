namespace IO_Panel.Server.Data.Entities;

/// <summary>
/// Persisted scene definition. A scene is a named batch of device state changes.
/// </summary>
public sealed class SceneEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// If true, the scene can be activated without authentication.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Actions executed when the scene is activated.
    /// </summary>
    public List<SceneActionEntity> Actions { get; set; } = new();
}   