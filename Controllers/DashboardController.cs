using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.Dashboard;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// Dashboard helper endpoints.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Returns aggregated counts for the home screen.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> Summary(CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetSummaryAsync(User.GetOrganizationId(), cancellationToken));
}
