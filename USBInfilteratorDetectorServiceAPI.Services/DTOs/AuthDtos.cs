namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record LoginRequest(string CorpCode, string Username, string Password);
public record RefreshRequest(string RefreshToken);
public record AuthTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
public record SuperAdminLoginRequest(string Username, string Password);

/// <summary>
/// Public forgot-password request (CorpCode + username). Always answered with a generic message.
/// </summary>
public record ForgotPasswordRequest(string CorpCode, string Username);

/// <summary>
/// Generic response for forgot-password to avoid user enumeration.
/// </summary>
public record ForgotPasswordResponse(string Message);

/// <summary>
/// Authenticated change-password request.
/// </summary>
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
