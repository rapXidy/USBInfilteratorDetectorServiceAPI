using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Services.Auth;

/// <summary>
/// Authenticates platform SuperAdmins configured in AppSettings.
/// </summary>
public class SuperAdminAuthService
{
    private readonly SuperAdminOptions _options;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public SuperAdminAuthService(
        IOptions<SuperAdminOptions> options,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _options = options.Value;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    /// <summary>
    /// Validates SuperAdmin credentials and issues a platform JWT.
    /// </summary>
    public AuthTokenResponse Login(SuperAdminLoginRequest request)
    {
        var account = _options.Accounts.FirstOrDefault(x =>
            string.Equals(x.Username, request.Username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (account is null || string.IsNullOrEmpty(account.Password))
            throw new ServiceException("Invalid credentials.", 401);

        var provided = Encoding.UTF8.GetBytes(request.Password);
        var expected = Encoding.UTF8.GetBytes(account.Password);
        if (provided.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(provided, expected))
            throw new ServiceException("Invalid credentials.", 401);

        var accessToken = _jwtTokenService.CreateSuperAdminAccessToken(account.Username);
        return new AuthTokenResponse(
            accessToken,
            string.Empty,
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes));
    }
}
