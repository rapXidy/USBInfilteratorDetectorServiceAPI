namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record LoginRequest(string CorpCode, string Username, string Password);
public record RefreshRequest(string RefreshToken);
public record AuthTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
public record SuperAdminLoginRequest(string Username, string Password);
