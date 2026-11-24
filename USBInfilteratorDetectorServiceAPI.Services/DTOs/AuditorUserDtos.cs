using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record CreateAuditorUserRequest(string Username, string Password, string DisplayName, AuditorRole Role, string? Email = null);
public record UpdateAuditorUserRequest(string DisplayName, AuditorRole Role, bool IsActive, string? Password, string? Email = null);
public record AuditorUserResponse(Guid Id, string Username, string DisplayName, string? Email, AuditorRole Role, bool IsActive, DateTime CreatedAt, DateTime? LastLoginAt);
