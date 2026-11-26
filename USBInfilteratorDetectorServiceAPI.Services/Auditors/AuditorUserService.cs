using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Security;

namespace USBInfilteratorDetectorServiceAPI.Services.Auditors;

/// <summary>
/// Manages allowed auditor users for an organization (Admin-only).
/// </summary>
public class AuditorUserService
{
    private readonly IAuditorRepository _auditors;
    private readonly IOrganizationRepository _organizations;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly INotificationEnqueueService _notifications;

    public AuditorUserService(
        IAuditorRepository auditors,
        IOrganizationRepository organizations,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        INotificationEnqueueService notifications)
    {
        _auditors = auditors;
        _organizations = organizations;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _notifications = notifications;
    }

    /// <summary>
    /// Lists auditor users for the organization.
    /// </summary>
    public async Task<IReadOnlyList<AuditorUserResponse>> ListAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var users = await _auditors.ListByOrganizationAsync(organizationId, cancellationToken);
        return users.Select(Map).ToList();
    }

    /// <summary>
    /// Adds an allowed auditor user with a server-generated password emailed to the auditor.
    /// </summary>
    public async Task<AuditorUserResponse> CreateAsync(Guid organizationId, CreateAuditorUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ServiceException("Username is required.");

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new ServiceException("Display name is required.");

        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email))
            throw new ServiceException("Email is required so the generated password can be delivered.");

        if (await _auditors.UsernameExistsAsync(organizationId, request.Username.Trim(), cancellationToken: cancellationToken))
            throw new ServiceException("Username already exists in this organization.", 409);

        var temporaryPassword = TemporaryPasswordGenerator.Generate();
        var auditor = new Auditor
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Username = request.Username.Trim(),
            PasswordHash = _passwordHasher.Hash(temporaryPassword),
            DisplayName = request.DisplayName.Trim(),
            Email = email,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _auditors.AddAsync(auditor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var organization = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);

        await _notifications.EnqueueAuditorWelcomeAsync(organization, auditor, temporaryPassword, cancellationToken);

        return Map(auditor);
    }

    /// <summary>
    /// Updates an auditor user profile, role, and active flag.
    /// </summary>
    public async Task<AuditorUserResponse> UpdateAsync(Guid organizationId, Guid auditorId, UpdateAuditorUserRequest request, CancellationToken cancellationToken = default)
    {
        var auditor = await _auditors.GetByIdAsync(organizationId, auditorId, cancellationToken)
            ?? throw new ServiceException("Auditor user not found.", 404);

        if (auditor.Role == AuditorRole.Admin && auditor.IsActive &&
            (request.Role != AuditorRole.Admin || !request.IsActive))
        {
            var activeAdmins = await _auditors.CountActiveAdminsAsync(organizationId, cancellationToken);
            if (activeAdmins <= 1)
                throw new ServiceException("Cannot demote or deactivate the last active Admin.", 409);
        }

        auditor.DisplayName = request.DisplayName.Trim();
        auditor.Email = NormalizeEmail(request.Email);
        auditor.Role = request.Role;
        auditor.IsActive = request.IsActive;
        auditor.UpdatedAt = DateTime.UtcNow;

        _auditors.Update(auditor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(auditor);
    }

    /// <summary>
    /// Generates a temporary password, emails it, and revokes existing refresh tokens.
    /// </summary>
    public async Task ResetPasswordAsync(Guid organizationId, Guid auditorId, CancellationToken cancellationToken = default)
    {
        var auditor = await _auditors.GetByIdAsync(organizationId, auditorId, cancellationToken)
            ?? throw new ServiceException("Auditor user not found.", 404);

        if (string.IsNullOrWhiteSpace(auditor.Email))
            throw new ServiceException("Auditor has no email address; cannot deliver a temporary password.");

        var organization = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);

        var temporaryPassword = TemporaryPasswordGenerator.Generate();
        auditor.PasswordHash = _passwordHasher.Hash(temporaryPassword);
        auditor.UpdatedAt = DateTime.UtcNow;
        _auditors.Update(auditor);
        await _refreshTokens.RevokeAllForAuditorAsync(auditor.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notifications.EnqueuePasswordResetAsync(organization, auditor, temporaryPassword, cancellationToken);
    }

    /// <summary>
    /// Soft-deactivates an auditor user.
    /// </summary>
    public async Task DeactivateAsync(Guid organizationId, Guid auditorId, CancellationToken cancellationToken = default)
    {
        var auditor = await _auditors.GetByIdAsync(organizationId, auditorId, cancellationToken)
            ?? throw new ServiceException("Auditor user not found.", 404);

        if (auditor.Role == AuditorRole.Admin && auditor.IsActive)
        {
            var activeAdmins = await _auditors.CountActiveAdminsAsync(organizationId, cancellationToken);
            if (activeAdmins <= 1)
                throw new ServiceException("Cannot deactivate the last active Admin.", 409);
        }

        auditor.IsActive = false;
        auditor.UpdatedAt = DateTime.UtcNow;
        _auditors.Update(auditor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeEmail(string? email)
        => string.IsNullOrWhiteSpace(email) ? null : email.Trim();

    private static AuditorUserResponse Map(Auditor auditor)
        => new(auditor.Id, auditor.Username, auditor.DisplayName, auditor.Email, auditor.Role, auditor.IsActive, auditor.CreatedAt, auditor.LastLoginAt);
}
