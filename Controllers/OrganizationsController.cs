using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Organizations;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// Tenant organization profile endpoints.
/// </summary>
[ApiController]
[Route("api/organizations")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly OrganizationService _organizationService;

    public OrganizationsController(OrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    /// <summary>
    /// Returns the current tenant organization profile.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<OrganizationProfileResponse>> Me(CancellationToken cancellationToken)
        => Ok(await _organizationService.GetCurrentAsync(User.GetOrganizationId(), cancellationToken));
}
