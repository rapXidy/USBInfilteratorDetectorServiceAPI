using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

/// <summary>
/// Create auditor request. Password is generated server-side and emailed to the auditor.
/// </summary>
public record CreateAuditorUserRequest(string Username, string DisplayName, AuditorRole Role, string Email);

/// <summary>
/// Update auditor profile. Password resets use POST .../reset-password (email temporary password).
/// </summary>
public record UpdateAuditorUserRequest(string DisplayName, AuditorRole Role, bool IsActive, string? Email = null);

public record AuditorUserResponse(Guid Id, string Username, string DisplayName, string? Email, AuditorRole Role, bool IsActive, DateTime CreatedAt, DateTime? LastLoginAt);
