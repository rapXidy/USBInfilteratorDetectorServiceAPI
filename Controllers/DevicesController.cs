using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.Devices;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// Allowed USB device management endpoints.
/// </summary>
[ApiController]
[Route("api/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly AllowedDeviceService _deviceService;

    public DevicesController(AllowedDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <summary>
    /// Lists allowed devices for the organization.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AllowedDeviceResponse>>> List(CancellationToken cancellationToken)
        => Ok(await _deviceService.ListAsync(User.GetOrganizationId(), cancellationToken));

    /// <summary>
    /// Adds an allowed device.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AuditorOrAbove")]
    public async Task<ActionResult<AllowedDeviceResponse>> Create([FromBody] CreateAllowedDeviceRequest request, CancellationToken cancellationToken)
        => Ok(await _deviceService.CreateAsync(User.GetOrganizationId(), User.GetAuditorId(), request, cancellationToken));

    /// <summary>
    /// Updates an allowed device.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AuditorOrAbove")]
    public async Task<ActionResult<AllowedDeviceResponse>> Update(Guid id, [FromBody] UpdateAllowedDeviceRequest request, CancellationToken cancellationToken)
        => Ok(await _deviceService.UpdateAsync(User.GetOrganizationId(), id, request, cancellationToken));

    /// <summary>
    /// Soft-deletes an allowed device.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AuditorOrAbove")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deviceService.DeleteAsync(User.GetOrganizationId(), id, cancellationToken);
        return NoContent();
    }
}
