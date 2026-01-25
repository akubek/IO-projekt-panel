namespace IO_Panel.Server.Services.Time;

public interface ITimeService
{
    Task<TimeSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    Task<TimeSnapshot> SetAsync(string timeZoneId, string virtualNowLocal, CancellationToken cancellationToken = default);

    Task<TimeSnapshot> ResetAsync(CancellationToken cancellationToken = default);
}

public sealed record TimeSnapshot(
    string TimeZoneId,
    DateTimeOffset NowUtc,
    DateTimeOffset NowLocal,
    DateTimeOffset AppliedAtUtc,
    DateTimeOffset VirtualNowAtAppliedUtc);