using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.Auth;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// Tenant auditor authentication endpoints.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Logs in an auditor with CorpCode, username, and password.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => Ok(await _authService.LoginAsync(request, cancellationToken));

    /// <summary>
    /// Requests a temporary password email for an auditor account (generic response).
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
        => Ok(await _authService.ForgotPasswordAsync(request, cancellationToken));

    /// <summary>
    /// Changes the signed-in auditor's password.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var auditorId = User.GetAuditorId()
            ?? throw new UnauthorizedAccessException("Auditor claim is missing.");
        await _authService.ChangePasswordAsync(User.GetOrganizationId(), auditorId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Refreshes access tokens using a refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokenResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        => Ok(await _authService.RefreshAsync(request, cancellationToken));

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);
        return NoContent();
    }
}
