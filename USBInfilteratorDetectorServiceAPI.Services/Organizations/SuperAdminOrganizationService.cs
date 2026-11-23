using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Services.Organizations;

/// <summary>
/// Platform-level organization management for SuperAdmins.
/// </summary>
public class SuperAdminOrganizationService
{
    private readonly IOrganizationRepository _organizations;
    private readonly IUnitOfWork _unitOfWork;

    public SuperAdminOrganizationService(IOrganizationRepository organizations, IUnitOfWork unitOfWork)
    {
        _organizations = organizations;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Lists organizations, optionally filtered by active status.
    /// </summary>
    public async Task<IReadOnlyList<SuperAdminOrganizationResponse>> ListAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var orgs = await _organizations.ListAsync(isActive, cancellationToken);
        return orgs.Select(Map).ToList();
    }

    /// <summary>
    /// Gets organization detail for SuperAdmin oversight.
    /// </summary>
    public async Task<SuperAdminOrganizationResponse> GetAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var org = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);
        return Map(org);
    }

    /// <summary>
    /// Activates an organization so auditors and Worker ingest can operate.
    /// </summary>
    public async Task<SuperAdminOrganizationResponse> ActivateAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var org = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);

        org.IsActive = true;
        _organizations.Update(org);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(org);
    }

    /// <summary>
    /// Deactivates an organization, blocking tenant login and ingest.
    /// </summary>
    public async Task<SuperAdminOrganizationResponse> DeactivateAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var org = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);

        org.IsActive = false;
        _organizations.Update(org);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(org);
    }

    private static SuperAdminOrganizationResponse Map(Domain.Entities.Organization org)
    {
        var prefix = org.ApiCredentials.FirstOrDefault(x => x.IsActive)?.ApiKeyPrefix;
        return new SuperAdminOrganizationResponse(
            org.Id,
            org.CorpCode,
            org.Name,
            org.Email,
            org.IsActive,
            org.OnboardingStatus,
            org.CreatedAt,
            org.OnboardingCompletedAt,
            prefix);
    }
}
