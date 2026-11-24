using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Services.Onboarding;

/// <summary>
/// Handles organization onboarding draft creation, updates, and completion.
/// </summary>
public class OnboardingService
{
    private readonly IOrganizationRepository _organizations;
    private readonly IAuditorRepository _auditors;
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IOrganizationApiCredentialRepository _credentials;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICorpCodeGenerator _corpCodeGenerator;
    private readonly IApiKeyGenerator _apiKeyGenerator;
    private readonly INotificationEnqueueService _notifications;

    public OnboardingService(
        IOrganizationRepository organizations,
        IAuditorRepository auditors,
        ISubscriptionRepository subscriptions,
        IOrganizationApiCredentialRepository credentials,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ICorpCodeGenerator corpCodeGenerator,
        IApiKeyGenerator apiKeyGenerator,
        INotificationEnqueueService notifications)
    {
        _organizations = organizations;
        _auditors = auditors;
        _subscriptions = subscriptions;
        _credentials = credentials;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _corpCodeGenerator = corpCodeGenerator;
        _apiKeyGenerator = apiKeyGenerator;
        _notifications = notifications;
    }

    /// <summary>
    /// Starts onboarding by creating a draft organization, admin auditor, and FreeTrial subscription.
    /// </summary>
    public async Task<OnboardingStatusResponse> StartAsync(StartOnboardingRequest request, CancellationToken cancellationToken = default)
    {
        ValidateStart(request);

        var now = DateTime.UtcNow;
        var organizationId = Guid.NewGuid();

        var organization = new Organization
        {
            Id = organizationId,
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone?.Trim(),
            OfficeAddress = request.OfficeAddress?.Trim(),
            Sector = request.Sector?.Trim(),
            CreatedAt = now,
            IsActive = false,
            OnboardingStatus = OnboardingStatus.Draft
        };

        var admin = new Auditor
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Username = request.AdminUsername.Trim(),
            PasswordHash = _passwordHasher.Hash(request.AdminPassword),
            DisplayName = request.AdminDisplayName.Trim(),
            Role = AuditorRole.Admin,
            IsActive = true,
            CreatedAt = now
        };

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Plan = SubscriptionPlan.FreeTrial,
            Status = SubscriptionStatus.Active,
            StartsAt = now,
            EndsAt = now.AddDays(30),
            CreatedAt = now
        };

        await _organizations.AddAsync(organization, cancellationToken);
        await _auditors.AddAsync(admin, cancellationToken);
        await _subscriptions.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapStatus(organization);
    }

    /// <summary>
    /// Updates draft organization profile fields before completion.
    /// </summary>
    public async Task<OnboardingStatusResponse> UpdateDraftAsync(Guid organizationId, UpdateOnboardingRequest request, CancellationToken cancellationToken = default)
    {
        var organization = await GetDraftOrThrowAsync(organizationId, cancellationToken);
        organization.Name = request.Name.Trim();
        organization.Email = request.Email.Trim();
        organization.Phone = request.Phone?.Trim();
        organization.OfficeAddress = request.OfficeAddress?.Trim();
        organization.Sector = request.Sector?.Trim();
        _organizations.Update(organization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapStatus(organization);
    }

    /// <summary>
    /// Returns the current onboarding status for an organization.
    /// </summary>
    public async Task<OnboardingStatusResponse> GetStatusAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var organization = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);
        return MapStatus(organization);
    }

    /// <summary>
    /// Completes onboarding by issuing CorpCode and Worker API key (returned once).
    /// </summary>
    public async Task<CompleteOnboardingResponse> CompleteAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var organization = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);

        if (organization.OnboardingStatus == OnboardingStatus.Completed)
            throw new ServiceException("Onboarding is already completed.", 409);

        if (organization.OnboardingStatus != OnboardingStatus.Draft)
            throw new ServiceException("Only draft organizations can be completed.", 409);

        string corpCode;
        do
        {
            corpCode = _corpCodeGenerator.Generate(organization.Name);
        } while (await _organizations.CorpCodeExistsAsync(corpCode, cancellationToken));

        var (apiKey, prefix) = _apiKeyGenerator.Generate();
        var now = DateTime.UtcNow;

        organization.CorpCode = corpCode;
        organization.OnboardingStatus = OnboardingStatus.Completed;
        organization.OnboardingCompletedAt = now;
        organization.IsActive = true;
        _organizations.Update(organization);

        await _credentials.AddAsync(new OrganizationApiCredential
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            ApiKeyHash = _passwordHasher.Hash(apiKey),
            ApiKeyPrefix = prefix,
            CreatedAt = now,
            IsActive = true
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notifications.EnqueueOnboardingCredentialsAsync(organization, corpCode, apiKey, cancellationToken);

        var auditors = await _auditors.ListByOrganizationAsync(organization.Id, cancellationToken);
        var adminUsername = auditors.FirstOrDefault(x => x.Role == AuditorRole.Admin)?.Username ?? string.Empty;

        return new CompleteOnboardingResponse(
            organization.Id,
            corpCode,
            apiKey,
            prefix,
            adminUsername,
            SubscriptionPlan.FreeTrial);
    }

    private async Task<Organization> GetDraftOrThrowAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var organization = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);

        if (organization.OnboardingStatus != OnboardingStatus.Draft)
            throw new ServiceException("Only draft organizations can be updated or completed.", 409);

        return organization;
    }

    private static void ValidateStart(StartOnboardingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ServiceException("Name is required.");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ServiceException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.AdminUsername))
            throw new ServiceException("AdminUsername is required.");
        if (string.IsNullOrWhiteSpace(request.AdminPassword) || request.AdminPassword.Length < 8)
            throw new ServiceException("AdminPassword must be at least 8 characters.");
        if (string.IsNullOrWhiteSpace(request.AdminDisplayName))
            throw new ServiceException("AdminDisplayName is required.");
    }

    private static OnboardingStatusResponse MapStatus(Organization organization)
        => new(organization.Id, organization.Name, organization.Email, organization.OnboardingStatus, organization.IsActive, organization.CorpCode);
}
