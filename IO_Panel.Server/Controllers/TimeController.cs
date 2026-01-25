using IO_Panel.Server.Services.Time;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IO_Panel.Server.Controllers;

[ApiController]
[Route("time")]
public sealed class TimeController : ControllerBase
{
    private readonly ITimeService _timeService;

    public TimeController(ITimeService timeService)
    {
        _timeService = timeService;
    }

    [HttpGet]
    public async Task<ActionResult<TimeSnapshot>> Get(CancellationToken cancellationToken)
    {
        var snapshot = await _timeService.GetSnapshotAsync(cancellationToken);
        return Ok(snapshot);
    }

    public sealed record SetTimeRequest(string? TimeZoneId, string VirtualNowLocal);

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

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<ActionResult<TimeSnapshot>> Reset(CancellationToken cancellationToken)
    {
        var snapshot = await _timeService.ResetAsync(cancellationToken);
        return Ok(snapshot);
    }
}