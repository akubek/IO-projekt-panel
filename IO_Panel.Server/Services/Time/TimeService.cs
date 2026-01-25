using System.Globalization;
using IO_Panel.Server.Data;
using IO_Panel.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Services.Time;

/// <summary>
/// Time provider supporting persisted configuration for a virtual clock (offset/fixed time).
/// Used by the UI and the automation scheduler to stay consistent.
/// </summary>
public sealed class TimeService : ITimeService
{
    private readonly AppDbContext _db;

    public TimeService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns the current virtual time snapshot. If no configuration exists, returns real-time (UTC).
    /// </summary>
    public async Task<TimeSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var cfg = await _db.TimeConfigurations
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (cfg is null)
        {
            var nowUtc = DateTimeOffset.UtcNow;
            return new TimeSnapshot(
                TimeZoneId: "UTC",
                NowUtc: nowUtc,
                NowLocal: nowUtc,
                AppliedAtUtc: nowUtc,
                VirtualNowAtAppliedUtc: nowUtc);
        }

        var nowUtcComputed = ComputeVirtualNowUtc(cfg, DateTimeOffset.UtcNow);

        var tzId = string.IsNullOrWhiteSpace(cfg.TimeZoneId) ? "UTC" : cfg.TimeZoneId;
        var tz = ResolveTimeZoneOrUtc(tzId);

        var nowLocalComputed = TimeZoneInfo.ConvertTime(nowUtcComputed, tz);

        return new TimeSnapshot(
            TimeZoneId: tz.Id,
            NowUtc: nowUtcComputed,
            NowLocal: nowLocalComputed,
            AppliedAtUtc: cfg.AppliedAtUtc,
            VirtualNowAtAppliedUtc: cfg.VirtualNowAtAppliedUtc);
    }

    /// <summary>
    /// Persists a new time configuration entry and returns the resulting snapshot.
    /// </summary>
    public async Task<TimeSnapshot> SetAsync(string timeZoneId, string virtualNowLocal, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(virtualNowLocal))
        {
            throw new InvalidOperationException("VirtualNowLocal is required.");
        }

        var tz = ResolveTimeZoneOrUtc(string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId);

        // Accept ISO 8601 (e.g. 2026-01-25T12:00:00.000Z or with offset) OR datetime-local (yyyy-MM-ddTHH:mm).
        DateTimeOffset virtualNowUtc;

        if (DateTime.TryParseExact(
                virtualNowLocal,
                "yyyy-MM-dd'T'HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var localWallClock))
        {
            localWallClock = DateTime.SpecifyKind(localWallClock, DateTimeKind.Unspecified);
            var offset = tz.GetUtcOffset(localWallClock);
            virtualNowUtc = new DateTimeOffset(localWallClock, offset).ToUniversalTime();
        }
        else if (DateTimeOffset.TryParse(
                     virtualNowLocal,
                     CultureInfo.InvariantCulture,
                     DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                     out var parsedDto))
        {
            virtualNowUtc = parsedDto.ToUniversalTime();
        }
        else
        {
            throw new InvalidOperationException(
                $"Invalid VirtualNowLocal: '{virtualNowLocal}'. Expected ISO-8601 (e.g. 2026-01-25T12:00:00Z) or yyyy-MM-ddTHH:mm.");
        }

        virtualNowUtc = virtualNowUtc.ToUniversalTime();

        var entity = new TimeConfigurationEntity
        {
            TimeZoneId = tz.Id,
            AppliedAtUtc = DateTimeOffset.UtcNow,
            VirtualNowAtAppliedUtc = virtualNowUtc
        };

        _db.TimeConfigurations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetSnapshotAsync(cancellationToken);
    }

    /// <summary>
    /// Removes any stored time configuration and returns the current snapshot (real-time behavior).
    /// </summary>
    public async Task<TimeSnapshot> ResetAsync(CancellationToken cancellationToken = default)
    {
        _db.TimeConfigurations.RemoveRange(_db.TimeConfigurations);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetSnapshotAsync(cancellationToken);
    }

    /// <summary>
    /// Computes the virtual "now" in UTC based on the persisted baseline and elapsed real time.
    /// </summary>
    private static DateTimeOffset ComputeVirtualNowUtc(TimeConfigurationEntity cfg, DateTimeOffset realNowUtc)
    {
        var elapsed = realNowUtc - cfg.AppliedAtUtc;
        return cfg.VirtualNowAtAppliedUtc + elapsed;
    }

    /// <summary>
    /// Attempts to resolve a time zone id; falls back to UTC if the id is unknown on the current OS.
    /// </summary>
    private static TimeZoneInfo ResolveTimeZoneOrUtc(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }
}