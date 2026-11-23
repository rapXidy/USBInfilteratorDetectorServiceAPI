using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Sessions;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// USB session query endpoints for auditors.
/// </summary>
[ApiController]
[Route("api/sessions")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly SessionService _sessionService;

    public SessionsController(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Lists sessions with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SessionListItemResponse>>> List(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int? minRiskScore,
        [FromQuery] string? host,
        [FromQuery] string? deviceSerial,
        [FromQuery] bool? detectionComplete,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _sessionService.QueryAsync(
            User.GetOrganizationId(), fromUtc, toUtc, minRiskScore, host, deviceSerial, detectionComplete, skip, take, cancellationToken));

    /// <summary>
    /// Returns full session detail including reasons and events.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionDetailResponse>> Get(Guid id, CancellationToken cancellationToken)
        => Ok(await _sessionService.GetByIdAsync(User.GetOrganizationId(), id, cancellationToken));
}
