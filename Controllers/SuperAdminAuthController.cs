using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using USBInfilteratorDetectorServiceAPI.Services.Auth;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// Platform SuperAdmin authentication endpoints.
/// </summary>
[ApiController]
[Route("api/superadmin/auth")]
public class SuperAdminAuthController : ControllerBase
{
    private readonly SuperAdminAuthService _authService;

    public SuperAdminAuthController(SuperAdminAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Logs in a SuperAdmin configured in AppSettings.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public ActionResult<AuthTokenResponse> Login([FromBody] SuperAdminLoginRequest request)
        => Ok(_authService.Login(request));
}
