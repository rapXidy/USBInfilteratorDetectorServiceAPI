using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Services.Auditors;

/// <summary>
/// Manages allowed auditor users for an organization (Admin-only).
/// </summary>
public class AuditorUserService
{
    private readonly IAuditorRepository _auditors;
    private readonly IOrganizationRepository _organizations;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly INotificationEnqueueService _notifications;

    public AuditorUserService(
        IAuditorRepository auditors,
        IOrganizationRepository organizations,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        INotificationEnqueueService notifications)
    {
        _auditors = auditors;
        _organizations = organizations;
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
    /// Adds an allowed auditor user to the organization.
    /// </summary>
    public async Task<AuditorUserResponse> CreateAsync(Guid organizationId, CreateAuditorUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ServiceException("Username and a password of at least 8 characters are required.");

        if (await _auditors.UsernameExistsAsync(organizationId, request.Username.Trim(), cancellationToken: cancellationToken))
            throw new ServiceException("Username already exists in this organization.", 409);

        var email = NormalizeEmail(request.Email);

        var auditor = new Auditor
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Username = request.Username.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            Email = email,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _auditors.AddAsync(auditor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(email))
        {
            var organization = await _organizations.GetByIdAsync(organizationId, cancellationToken);
            if (organization is not null)
                await _notifications.EnqueueAuditorWelcomeAsync(organization, auditor, cancellationToken);
        }

        return Map(auditor);
    }

    /// <summary>
    /// Updates an auditor user profile, role, active flag, and optional password.
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

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 8)
                throw new ServiceException("Password must be at least 8 characters.");
            auditor.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        _auditors.Update(auditor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(auditor);
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
