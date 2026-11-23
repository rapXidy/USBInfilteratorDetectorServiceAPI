namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record IngestBatchRequest(IReadOnlyList<IngestPayloadDto> Items);

public record IngestPayloadDto(
    string CorpCode,
    IngestHostDto Host,
    IngestSessionDto Session,
    IReadOnlyList<IngestEventDto> Events,
    IngestAuditSettingsDto AuditSettingsStatus);

public record IngestHostDto(string HostName, string DeviceId, string? LoggedInUser, string? OsVersion);

public record IngestSessionDto(
    Guid SessionId,
    string DeviceSerial,
    string? Vendor,
    string? Model,
    DateTime ConnectedAt,
    DateTime? DisconnectedAt,
    long TotalBytesWritten,
    int RiskScore,
    IReadOnlyList<IngestReasonDto> Reasons);

public record IngestReasonDto(string Rule, int Weight, string Description, IReadOnlyList<long> EventRecordIds);

public record IngestEventDto(int EventId, long RecordId, DateTime TimeCreated, string Activity, string? Description);

public record IngestAuditSettingsDto(bool PnpActivityEnabled, bool RemovableStorageEnabled, bool DetectionComplete);

public record IngestBatchResponse(int Accepted, int Skipped);
