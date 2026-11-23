namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record SessionListItemResponse(
    Guid Id,
    Guid SessionId,
    string HostName,
    string DeviceSerial,
    string? Vendor,
    string? Model,
    DateTime ConnectedAt,
    DateTime? DisconnectedAt,
    long TotalBytesWritten,
    int RiskScore,
    bool DetectionComplete);

public record SessionReasonResponse(string RuleName, int Weight, string Description, IReadOnlyList<long> EventRecordIds);

public record RawEventResponse(int EventId, long RecordId, DateTime TimeCreated, string Activity, string? Description);

public record SessionDetailResponse(
    Guid Id,
    Guid SessionId,
    string HostName,
    string? LoggedInUser,
    string DeviceSerial,
    string? Vendor,
    string? Model,
    DateTime ConnectedAt,
    DateTime? DisconnectedAt,
    long TotalBytesWritten,
    int RiskScore,
    bool DetectionComplete,
    IReadOnlyList<SessionReasonResponse> Reasons,
    IReadOnlyList<RawEventResponse> Events);

public record AlertResponse(
    Guid SessionId,
    Guid UsbSessionId,
    string HostName,
    string DeviceSerial,
    int RiskScore,
    bool DetectionComplete,
    DateTime ConnectedAt,
    IReadOnlyList<SessionReasonResponse> TopReasons);
