namespace IO_Panel.Server.Services.Time;

/// <summary>
/// Provides a unified time source for the application.
/// Supports a configurable virtual clock (used by UI and automation scheduling) backed by persisted configuration.
/// </summary>
public interface ITimeService
{
    /// <summary>
    /// Returns a snapshot of the current time (UTC and local) according to the active time configuration.
    /// </summary>
    Task<TimeSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the virtual clock using a configured time zone and a user-provided local "now" value.
    /// </summary>
    Task<TimeSnapshot> SetAsync(string timeZoneId, string virtualNowLocal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the time configuration back to real-time behavior.
    /// </summary>
    Task<TimeSnapshot> ResetAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Immutable view of the current time state used by UI and automation evaluation.
/// </summary>
public sealed record TimeSnapshot(
    string TimeZoneId,
    DateTimeOffset NowUtc,
    DateTimeOffset NowLocal,
    DateTimeOffset AppliedAtUtc,
    DateTimeOffset VirtualNowAtAppliedUtc);