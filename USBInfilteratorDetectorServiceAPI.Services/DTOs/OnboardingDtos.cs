using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record StartOnboardingRequest(
    string Name,
    string Email,
    string? Phone,
    string? OfficeAddress,
    string? Sector,
    string AdminUsername,
    string AdminPassword,
    string AdminDisplayName);

public record UpdateOnboardingRequest(
    string Name,
    string Email,
    string? Phone,
    string? OfficeAddress,
    string? Sector);

public record OnboardingStatusResponse(
    Guid OrganizationId,
    string Name,
    string Email,
    OnboardingStatus Status,
    bool IsActive,
    string? CorpCode);

public record CompleteOnboardingResponse(
    Guid OrganizationId,
    string CorpCode,
    string ApiKey,
    string ApiKeyPrefix,
    string AdminUsername,
    SubscriptionPlan SubscriptionPlan);
