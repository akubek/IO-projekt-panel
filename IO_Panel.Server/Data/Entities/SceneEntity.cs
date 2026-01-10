namespace IO_Panel.Server.Data.Entities;

public sealed class SceneEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPublic { get; set; }

    public List<SceneActionEntity> Actions { get; set; } = new();
}