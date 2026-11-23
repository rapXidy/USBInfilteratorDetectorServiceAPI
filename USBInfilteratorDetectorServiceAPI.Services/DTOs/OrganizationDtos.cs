using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record OrganizationProfileResponse(
    Guid Id,
    string? CorpCode,
    string Name,
    string Email,
    string? Phone,
    string? OfficeAddress,
    string? Sector,
    bool IsActive,
    OnboardingStatus OnboardingStatus,
    string? ApiKeyPrefix);

public record SuperAdminOrganizationResponse(
    Guid Id,
    string? CorpCode,
    string Name,
    string Email,
    bool IsActive,
    OnboardingStatus OnboardingStatus,
    DateTime CreatedAt,
    DateTime? OnboardingCompletedAt,
    string? ApiKeyPrefix);
