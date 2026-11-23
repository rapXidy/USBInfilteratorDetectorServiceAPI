using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.Auditors;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// Organization Admin endpoints for managing allowed auditor users.
/// </summary>
[ApiController]
[Route("api/auditor-users")]
[Authorize(Policy = "OrgAdminOnly")]
public class AuditorUsersController : ControllerBase
{
    private readonly AuditorUserService _auditorUserService;

    public AuditorUsersController(AuditorUserService auditorUserService)
    {
        _auditorUserService = auditorUserService;
    }

    /// <summary>
    /// Lists allowed auditor users for the current organization.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditorUserResponse>>> List(CancellationToken cancellationToken)
        => Ok(await _auditorUserService.ListAsync(User.GetOrganizationId(), cancellationToken));

    /// <summary>
    /// Adds an allowed auditor user.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AuditorUserResponse>> Create([FromBody] CreateAuditorUserRequest request, CancellationToken cancellationToken)
        => Ok(await _auditorUserService.CreateAsync(User.GetOrganizationId(), request, cancellationToken));

    /// <summary>
    /// Updates an allowed auditor user.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AuditorUserResponse>> Update(Guid id, [FromBody] UpdateAuditorUserRequest request, CancellationToken cancellationToken)
        => Ok(await _auditorUserService.UpdateAsync(User.GetOrganizationId(), id, request, cancellationToken));

    /// <summary>
    /// Soft-deactivates an auditor user.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _auditorUserService.DeactivateAsync(User.GetOrganizationId(), id, cancellationToken);
        return NoContent();
    }
}
