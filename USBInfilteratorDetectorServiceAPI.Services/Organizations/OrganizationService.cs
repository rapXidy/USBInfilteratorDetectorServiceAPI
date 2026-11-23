using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Services.Organizations;

/// <summary>
/// Tenant-facing organization profile reads.
/// </summary>
public class OrganizationService
{
    private readonly IOrganizationRepository _organizations;

    public OrganizationService(IOrganizationRepository organizations)
    {
        _organizations = organizations;
    }

    /// <summary>
    /// Returns the current tenant organization profile (never includes raw API key).
    /// </summary>
    public async Task<OrganizationProfileResponse> GetCurrentAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var org = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);

        var prefix = org.ApiCredentials.FirstOrDefault(x => x.IsActive)?.ApiKeyPrefix;
        return new OrganizationProfileResponse(
            org.Id,
            org.CorpCode,
            org.Name,
            org.Email,
            org.Phone,
            org.OfficeAddress,
            org.Sector,
            org.IsActive,
            org.OnboardingStatus,
            prefix);
    }
}
