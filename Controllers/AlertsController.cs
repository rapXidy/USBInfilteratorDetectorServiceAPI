using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Sessions;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// High-severity alert endpoints for auditors.
/// </summary>
[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly AlertService _alertService;

    public AlertsController(AlertService alertService)
    {
        _alertService = alertService;
    }

    /// <summary>
    /// Returns recent high-risk session alerts.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> List([FromQuery] int take = 50, CancellationToken cancellationToken = default)
        => Ok(await _alertService.GetRecentAsync(User.GetOrganizationId(), take, cancellationToken));
}
