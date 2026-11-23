using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.HrStaff;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// HR staff management and leaver USB-session correlation endpoints.
/// </summary>
[ApiController]
[Route("api/hr-staff")]
[Authorize]
public class HrStaffController : ControllerBase
{
    private readonly HrStaffService _hrStaffService;

    public HrStaffController(HrStaffService hrStaffService)
    {
        _hrStaffService = hrStaffService;
    }

    /// <summary>
    /// Lists HR staff for the current organization.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HrStaffResponse>>> List(
        [FromQuery] string? department,
        [FromQuery] bool? resigningOnly,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
        => Ok(await _hrStaffService.ListAsync(User.GetOrganizationId(), department, resigningOnly, isActive, cancellationToken));

    /// <summary>
    /// Returns organization HR summary counts (resigning staff + recent leaver sessions).
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<HrStaffSummaryResponse>> Summary(CancellationToken cancellationToken)
        => Ok(await _hrStaffService.GetSummaryAsync(User.GetOrganizationId(), cancellationToken));

    /// <summary>
    /// Lists USB sessions for resigning/exited staff.
    /// </summary>
    [HttpGet("leavers/sessions")]
    [Authorize(Policy = "AuditorOrAbove")]
    public async Task<ActionResult<IReadOnlyList<HrStaffLeaverSessionResponse>>> LeaverSessions(
        [FromQuery] bool includeExited = false,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] int? minRiskScore = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _hrStaffService.GetLeaverSessionsAsync(
            User.GetOrganizationId(), includeExited, fromUtc, toUtc, minRiskScore, skip, take, cancellationToken));

    /// <summary>
    /// Gets a single HR staff member.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HrStaffResponse>> Get(Guid id, CancellationToken cancellationToken)
        => Ok(await _hrStaffService.GetByIdAsync(User.GetOrganizationId(), id, cancellationToken));

    /// <summary>
    /// Lists USB sessions correlated to one staff member.
    /// </summary>
    [HttpGet("{id:guid}/sessions")]
    [Authorize(Policy = "AuditorOrAbove")]
    public async Task<ActionResult<IReadOnlyList<SessionListItemResponse>>> StaffSessions(
        Guid id,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] int? minRiskScore = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _hrStaffService.GetStaffSessionsAsync(
            User.GetOrganizationId(), id, fromUtc, toUtc, minRiskScore, skip, take, cancellationToken));

    /// <summary>
    /// Creates an HR staff record.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "OrgAdminOnly")]
    public async Task<ActionResult<HrStaffResponse>> Create([FromBody] CreateHrStaffRequest request, CancellationToken cancellationToken)
        => Ok(await _hrStaffService.CreateAsync(User.GetOrganizationId(), User.GetAuditorId(), request, cancellationToken));

    /// <summary>
    /// Updates an HR staff record.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "OrgAdminOnly")]
    public async Task<ActionResult<HrStaffResponse>> Update(Guid id, [FromBody] UpdateHrStaffRequest request, CancellationToken cancellationToken)
        => Ok(await _hrStaffService.UpdateAsync(User.GetOrganizationId(), id, request, cancellationToken));

    /// <summary>
    /// Soft-deactivates an HR staff record.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "OrgAdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _hrStaffService.DeactivateAsync(User.GetOrganizationId(), id, cancellationToken);
        return NoContent();
    }
}
