namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public enum HrStaffStatus
{
    Active = 0,
    Resigning = 1,
    Exited = 2
}

public record CreateHrStaffRequest(
    string FullName,
    string Department,
    string? PhoneNumber,
    string? Email,
    string? JobTitle,
    string? EmployeeNumber,
    string? WindowsUsername,
    string? DeviceId,
    DateTime DateEmployed,
    DateTime? DateOfResignation,
    DateTime? DateOfExit,
    string? Notes);

public record UpdateHrStaffRequest(
    string FullName,
    string Department,
    string? PhoneNumber,
    string? Email,
    string? JobTitle,
    string? EmployeeNumber,
    string? WindowsUsername,
    string? DeviceId,
    DateTime DateEmployed,
    DateTime? DateOfResignation,
    DateTime? DateOfExit,
    bool IsActive,
    string? Notes);

public record HrStaffResponse(
    Guid Id,
    string FullName,
    string Department,
    string? PhoneNumber,
    string? Email,
    string? JobTitle,
    string? EmployeeNumber,
    string? WindowsUsername,
    string? DeviceId,
    DateTime DateEmployed,
    DateTime? DateOfResignation,
    DateTime? DateOfExit,
    bool IsActive,
    string? Notes,
    HrStaffStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record HrStaffLeaverSessionResponse(
    Guid StaffId,
    string StaffFullName,
    string Department,
    HrStaffStatus StaffStatus,
    Guid SessionId,
    Guid UsbSessionId,
    string HostName,
    string? LoggedInUser,
    string HostDeviceId,
    string DeviceSerial,
    DateTime ConnectedAt,
    DateTime? DisconnectedAt,
    long TotalBytesWritten,
    int RiskScore,
    bool DetectionComplete);

public record HrStaffSummaryResponse(int ResigningCount, int RecentLeaverSessionCount);
