using System.Security.Claims;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Services.Abstractions;

/// <summary>
/// Issues and describes JWT access tokens for auditors and SuperAdmins.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Creates an access token for a tenant auditor.
    /// </summary>
    string CreateAuditorAccessToken(Auditor auditor, Organization organization);

    /// <summary>
    /// Creates an access token for a platform SuperAdmin.
    /// </summary>
    string CreateSuperAdminAccessToken(string username);

    /// <summary>
    /// Creates a cryptographically random refresh token string.
    /// </summary>
    string CreateRefreshToken();

    /// <summary>
    /// Hashes a refresh token for storage.
    /// </summary>
    string HashRefreshToken(string refreshToken);
}
