using System.Security.Claims;
using USBInfilteratorDetectorServiceAPI.Infrastructure.Security;

namespace USBInfilteratorDetectorServiceAPI.Auth;

/// <summary>
/// Helpers for reading tenant claims from the authenticated principal.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Reads the organization id claim required for tenant-scoped endpoints.
    /// </summary>
    public static Guid GetOrganizationId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtTokenService.ClaimOrganizationId);
        if (!Guid.TryParse(value, out var organizationId))
            throw new UnauthorizedAccessException("Organization claim is missing.");
        return organizationId;
    }

    /// <summary>
    /// Reads the auditor id claim when present.
    /// </summary>
    public static Guid? GetAuditorId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtTokenService.ClaimAuditorId);
        return Guid.TryParse(value, out var auditorId) ? auditorId : null;
    }
}
