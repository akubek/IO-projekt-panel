namespace IO_Panel.Server.Data.Entities;

/// <summary>
/// Persisted virtual time configuration used by <c>TimeService</c>.
/// Allows the UI and automation scheduler to operate against a controlled clock.
/// </summary>
public sealed class TimeConfigurationEntity
{
    /// <summary>
    /// Singleton row key (enforced by unique index).
    /// </summary>
    public int Id { get; set; }

    public string TimeZoneId { get; set; } = "UTC";

    /// <summary>
    /// When the configuration was applied (real UTC time).
    /// </summary>
    public DateTimeOffset AppliedAtUtc { get; set; }

    /// <summary>
    /// The virtual "now" corresponding to <see cref="AppliedAtUtc"/>.
    /// </summary>
    public DateTimeOffset VirtualNowAtAppliedUtc { get; set; }
}