using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Organizations;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// SuperAdmin organization management endpoints.
/// </summary>
[ApiController]
[Route("api/superadmin/organizations")]
[Authorize(Policy = "SuperAdminOnly")]
public class SuperAdminOrganizationsController : ControllerBase
{
    private readonly SuperAdminOrganizationService _organizationService;

    public SuperAdminOrganizationsController(SuperAdminOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    /// <summary>
    /// Lists all organizations.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SuperAdminOrganizationResponse>>> List([FromQuery] bool? isActive, CancellationToken cancellationToken)
        => Ok(await _organizationService.ListAsync(isActive, cancellationToken));

    /// <summary>
    /// Gets organization detail.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SuperAdminOrganizationResponse>> Get(Guid id, CancellationToken cancellationToken)
        => Ok(await _organizationService.GetAsync(id, cancellationToken));

    /// <summary>
    /// Activates an organization.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<SuperAdminOrganizationResponse>> Activate(Guid id, CancellationToken cancellationToken)
        => Ok(await _organizationService.ActivateAsync(id, cancellationToken));

    /// <summary>
    /// Deactivates an organization.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<SuperAdminOrganizationResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
        => Ok(await _organizationService.DeactivateAsync(id, cancellationToken));
}
