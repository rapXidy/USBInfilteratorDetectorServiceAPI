namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record CreateAllowedDeviceRequest(string DeviceSerial, string? Vendor, string? Model, string? Description);
public record UpdateAllowedDeviceRequest(string? Vendor, string? Model, string? Description, bool IsActive);
public record AllowedDeviceResponse(Guid Id, string DeviceSerial, string? Vendor, string? Model, string? Description, bool IsActive, DateTime CreatedAt);
