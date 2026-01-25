using System.Globalization;
using IO_Panel.Server.Data;
using IO_Panel.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IO_Panel.Server.Services.Time;

public sealed class TimeService : ITimeService
{
    private readonly AppDbContext _db;

    public TimeService(AppDbContext db)
    {
        _db = db;
    }

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

        return new TimeSnapshot(
            TimeZoneId: "UTC",
            NowUtc: nowUtcComputed,
            NowLocal: nowUtcComputed,
            AppliedAtUtc: cfg.AppliedAtUtc,
            VirtualNowAtAppliedUtc: cfg.VirtualNowAtAppliedUtc);
    }

    public async Task<TimeSnapshot> SetAsync(string timeZoneId, string virtualNowLocal, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(virtualNowLocal))
        {
            throw new InvalidOperationException("VirtualNowLocal is required.");
        }

        // Accept ISO 8601 (e.g. 2026-01-25T12:00:00.000Z or with offset) OR datetime-local (yyyy-MM-ddTHH:mm).
        DateTimeOffset virtualNowUtc;

        if (DateTimeOffset.TryParse(
                virtualNowLocal,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedDto))
        {
            virtualNowUtc = parsedDto;
        }
        else if (DateTime.TryParseExact(
                     virtualNowLocal,
                     "yyyy-MM-dd'T'HH:mm",
                     CultureInfo.InvariantCulture,
                     DateTimeStyles.None,
                     out var utcWallClock))
        {
            utcWallClock = DateTime.SpecifyKind(utcWallClock, DateTimeKind.Utc);
            virtualNowUtc = new DateTimeOffset(utcWallClock);
        }
        else
        {
            throw new InvalidOperationException(
                $"Invalid VirtualNowLocal: '{virtualNowLocal}'. Expected ISO-8601 (e.g. 2026-01-25T12:00:00Z) or yyyy-MM-ddTHH:mm.");
        }

        var entity = new TimeConfigurationEntity
        {
            TimeZoneId = "UTC",
            AppliedAtUtc = DateTimeOffset.UtcNow,
            VirtualNowAtAppliedUtc = virtualNowUtc
        };

        _db.TimeConfigurations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetSnapshotAsync(cancellationToken);
    }

    public async Task<TimeSnapshot> ResetAsync(CancellationToken cancellationToken = default)
    {
        _db.TimeConfigurations.RemoveRange(_db.TimeConfigurations);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetSnapshotAsync(cancellationToken);
    }

    private static DateTimeOffset ComputeVirtualNowUtc(TimeConfigurationEntity cfg, DateTimeOffset realNowUtc)
    {
        var elapsed = realNowUtc - cfg.AppliedAtUtc;
        return cfg.VirtualNowAtAppliedUtc + elapsed;
    }
}