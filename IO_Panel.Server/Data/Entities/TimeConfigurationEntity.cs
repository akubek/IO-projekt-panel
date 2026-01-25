namespace IO_Panel.Server.Data.Entities;

public sealed class TimeConfigurationEntity
{
    public int Id { get; set; }

    public string TimeZoneId { get; set; } = "UTC";

    public DateTimeOffset AppliedAtUtc { get; set; }

    public DateTimeOffset VirtualNowAtAppliedUtc { get; set; }
}  