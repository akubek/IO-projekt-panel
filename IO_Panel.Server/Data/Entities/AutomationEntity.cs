namespace IO_Panel.Server.Data.Entities;

public sealed class AutomationEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string LogicDefinition { get; set; } = string.Empty;
}