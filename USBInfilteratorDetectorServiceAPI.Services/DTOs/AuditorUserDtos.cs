using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record CreateAuditorUserRequest(string Username, string Password, string DisplayName, AuditorRole Role);
public record UpdateAuditorUserRequest(string DisplayName, AuditorRole Role, bool IsActive, string? Password);
public record AuditorUserResponse(Guid Id, string Username, string DisplayName, AuditorRole Role, bool IsActive, DateTime CreatedAt, DateTime? LastLoginAt);
