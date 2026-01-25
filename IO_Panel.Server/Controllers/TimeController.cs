using IO_Panel.Server.Services.Time;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IO_Panel.Server.Controllers;

/// <summary>
/// Exposes server time snapshots and admin-only time configuration.
/// </summary>
[ApiController]
[Route("time")]
public sealed class TimeController : ControllerBase
{
    private readonly ITimeService _timeService;

    public TimeController(ITimeService timeService)
    {
        _timeService = timeService;
    }

    /// <summary>
    /// Returns the current time snapshot used by the UI and automation scheduler.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<TimeSnapshot>> Get(CancellationToken cancellationToken)
    {
        var snapshot = await _timeService.GetSnapshotAsync(cancellationToken);
        return Ok(snapshot);
    }

    /// <summary>
    /// Request payload for time configuration.
    /// </summary>
    public sealed record SetTimeRequest(string? TimeZoneId, string VirtualNowLocal);

    /// <summary>
    /// Admin-only. Updates the configured time (virtual clock) used by the application.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult<TimeSnapshot>> Set([FromBody] SetTimeRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        if (string.IsNullOrWhiteSpace(request.VirtualNowLocal))
        {
            return BadRequest("VirtualNowLocal is required.");
        }

        var snapshot = await _timeService.SetAsync(request.TimeZoneId ?? "UTC", request.VirtualNowLocal, cancellationToken);
        return Ok(snapshot);
    }

    /// <summary>
    /// Admin-only. Resets time configuration back to real-time behavior.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<ActionResult<TimeSnapshot>> Reset(CancellationToken cancellationToken)
    {
        var snapshot = await _timeService.ResetAsync(cancellationToken);
        return Ok(snapshot);
    }
}