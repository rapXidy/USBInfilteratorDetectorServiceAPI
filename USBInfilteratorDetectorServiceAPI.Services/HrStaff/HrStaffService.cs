using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Services.HrStaff;

/// <summary>
/// Manages organization HR staff records and leaver USB-session correlation.
/// </summary>
public class HrStaffService
{
    private readonly IHrStaffRepository _hrStaff;
    private readonly IUnitOfWork _unitOfWork;

    public HrStaffService(IHrStaffRepository hrStaff, IUnitOfWork unitOfWork)
    {
        _hrStaff = hrStaff;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Lists HR staff for the organization with optional filters.
    /// </summary>
    public async Task<IReadOnlyList<HrStaffResponse>> ListAsync(
        Guid organizationId,
        string? department = null,
        bool? resigningOnly = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var staff = await _hrStaff.ListAsync(organizationId, department, resigningOnly, isActive, cancellationToken);
        return staff.Select(Map).ToList();
    }

    /// <summary>
    /// Gets a single HR staff record.
    /// </summary>
    public async Task<HrStaffResponse> GetByIdAsync(Guid organizationId, Guid staffId, CancellationToken cancellationToken = default)
    {
        var staff = await _hrStaff.GetByIdAsync(organizationId, staffId, cancellationToken)
            ?? throw new ServiceException("HR staff member not found.", 404);
        return Map(staff);
    }

    /// <summary>
    /// Creates an HR staff record.
    /// </summary>
    public async Task<HrStaffResponse> CreateAsync(
        Guid organizationId,
        Guid? auditorId,
        CreateHrStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request.FullName, request.Department, request.DateEmployed, request.DateOfResignation, request.DateOfExit);

        var employeeNumber = NormalizeOptional(request.EmployeeNumber);
        if (employeeNumber is not null
            && await _hrStaff.EmployeeNumberExistsAsync(organizationId, employeeNumber, cancellationToken: cancellationToken))
        {
            throw new ServiceException("Employee number already exists in this organization.", 409);
        }

        var staff = new Domain.Entities.HrStaff
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            FullName = request.FullName.Trim(),
            Department = request.Department.Trim(),
            PhoneNumber = NormalizeOptional(request.PhoneNumber),
            Email = NormalizeOptional(request.Email),
            JobTitle = NormalizeOptional(request.JobTitle),
            EmployeeNumber = employeeNumber,
            WindowsUsername = NormalizeOptional(request.WindowsUsername),
            DeviceId = NormalizeOptional(request.DeviceId),
            DateEmployed = DateTime.SpecifyKind(request.DateEmployed, DateTimeKind.Utc),
            DateOfResignation = ToUtc(request.DateOfResignation),
            DateOfExit = ToUtc(request.DateOfExit),
            IsActive = true,
            Notes = NormalizeOptional(request.Notes),
            CreatedAt = DateTime.UtcNow,
            CreatedByAuditorId = auditorId
        };

        await _hrStaff.AddAsync(staff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(staff);
    }

    /// <summary>
    /// Updates an HR staff record including resignation and exit dates.
    /// </summary>
    public async Task<HrStaffResponse> UpdateAsync(
        Guid organizationId,
        Guid staffId,
        UpdateHrStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request.FullName, request.Department, request.DateEmployed, request.DateOfResignation, request.DateOfExit);

        var staff = await _hrStaff.GetByIdAsync(organizationId, staffId, cancellationToken)
            ?? throw new ServiceException("HR staff member not found.", 404);

        var employeeNumber = NormalizeOptional(request.EmployeeNumber);
        if (employeeNumber is not null
            && await _hrStaff.EmployeeNumberExistsAsync(organizationId, employeeNumber, staffId, cancellationToken))
        {
            throw new ServiceException("Employee number already exists in this organization.", 409);
        }

        staff.FullName = request.FullName.Trim();
        staff.Department = request.Department.Trim();
        staff.PhoneNumber = NormalizeOptional(request.PhoneNumber);
        staff.Email = NormalizeOptional(request.Email);
        staff.JobTitle = NormalizeOptional(request.JobTitle);
        staff.EmployeeNumber = employeeNumber;
        staff.WindowsUsername = NormalizeOptional(request.WindowsUsername);
        staff.DeviceId = NormalizeOptional(request.DeviceId);
        staff.DateEmployed = DateTime.SpecifyKind(request.DateEmployed, DateTimeKind.Utc);
        staff.DateOfResignation = ToUtc(request.DateOfResignation);
        staff.DateOfExit = ToUtc(request.DateOfExit);
        staff.IsActive = request.IsActive;
        staff.Notes = NormalizeOptional(request.Notes);
        staff.UpdatedAt = DateTime.UtcNow;

        _hrStaff.Update(staff);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(staff);
    }

    /// <summary>
    /// Soft-deactivates an HR staff record.
    /// </summary>
    public async Task DeactivateAsync(Guid organizationId, Guid staffId, CancellationToken cancellationToken = default)
    {
        var staff = await _hrStaff.GetByIdAsync(organizationId, staffId, cancellationToken)
            ?? throw new ServiceException("HR staff member not found.", 404);

        staff.IsActive = false;
        staff.UpdatedAt = DateTime.UtcNow;
        _hrStaff.Update(staff);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Lists USB sessions correlated to a single staff member.
    /// </summary>
    public async Task<IReadOnlyList<SessionListItemResponse>> GetStaffSessionsAsync(
        Guid organizationId,
        Guid staffId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int? minRiskScore,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var staff = await _hrStaff.GetByIdAsync(organizationId, staffId, cancellationToken)
            ?? throw new ServiceException("HR staff member not found.", 404);

        if (string.IsNullOrWhiteSpace(staff.WindowsUsername) && string.IsNullOrWhiteSpace(staff.DeviceId))
            return Array.Empty<SessionListItemResponse>();

        take = Math.Clamp(take <= 0 ? 50 : take, 1, 200);
        skip = Math.Max(0, skip);

        var sessions = await _hrStaff.QuerySessionsForStaffAsync(
            organizationId, staff, fromUtc, toUtc, minRiskScore, skip, take, cancellationToken);

        return sessions.Select(MapSession).ToList();
    }

    /// <summary>
    /// Lists USB sessions for resigning (and optionally exited) staff.
    /// </summary>
    public async Task<IReadOnlyList<HrStaffLeaverSessionResponse>> GetLeaverSessionsAsync(
        Guid organizationId,
        bool includeExited,
        DateTime? fromUtc,
        DateTime? toUtc,
        int? minRiskScore,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take <= 0 ? 50 : take, 1, 200);
        skip = Math.Max(0, skip);

        var rows = await _hrStaff.QueryLeaverSessionsAsync(
            organizationId, includeExited, fromUtc, toUtc, minRiskScore, skip, take, cancellationToken);

        return rows.Select(x => new HrStaffLeaverSessionResponse(
            x.Staff.Id,
            x.Staff.FullName,
            x.Staff.Department,
            ComputeStatus(x.Staff),
            x.Session.SessionId,
            x.Session.Id,
            x.Session.Host.HostName,
            x.Session.Host.LoggedInUser,
            x.Session.Host.DeviceId,
            x.Session.DeviceSerial,
            x.Session.ConnectedAt,
            x.Session.DisconnectedAt,
            x.Session.TotalBytesWritten,
            x.Session.RiskScore,
            x.Session.DetectionComplete)).ToList();
    }

    /// <summary>
    /// Returns organization-level HR summary counts for the Organization page.
    /// </summary>
    public async Task<HrStaffSummaryResponse> GetSummaryAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var resigning = await _hrStaff.CountResigningAsync(organizationId, cancellationToken);
        var since = DateTime.UtcNow.AddDays(-30);
        var sessions = await _hrStaff.CountRecentResigningSessionsAsync(organizationId, since, cancellationToken);
        return new HrStaffSummaryResponse(resigning, sessions);
    }

    private static void Validate(
        string fullName,
        string department,
        DateTime dateEmployed,
        DateTime? dateOfResignation,
        DateTime? dateOfExit)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ServiceException("FullName is required.");
        if (string.IsNullOrWhiteSpace(department))
            throw new ServiceException("Department is required.");
        if (dateEmployed == default)
            throw new ServiceException("DateEmployed is required.");
        if (dateOfResignation.HasValue && dateOfResignation.Value.Date < dateEmployed.Date)
            throw new ServiceException("DateOfResignation cannot be before DateEmployed.");
        if (dateOfExit.HasValue && !dateOfResignation.HasValue)
            throw new ServiceException("DateOfExit requires DateOfResignation.");
        if (dateOfExit.HasValue && dateOfResignation.HasValue && dateOfExit.Value.Date < dateOfResignation.Value.Date)
            throw new ServiceException("DateOfExit cannot be before DateOfResignation.");
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime? ToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static HrStaffStatus ComputeStatus(Domain.Entities.HrStaff staff)
    {
        if (staff.DateOfExit.HasValue)
            return HrStaffStatus.Exited;
        if (staff.DateOfResignation.HasValue)
            return HrStaffStatus.Resigning;
        return HrStaffStatus.Active;
    }

    private static HrStaffResponse Map(Domain.Entities.HrStaff staff)
        => new(
            staff.Id,
            staff.FullName,
            staff.Department,
            staff.PhoneNumber,
            staff.Email,
            staff.JobTitle,
            staff.EmployeeNumber,
            staff.WindowsUsername,
            staff.DeviceId,
            staff.DateEmployed,
            staff.DateOfResignation,
            staff.DateOfExit,
            staff.IsActive,
            staff.Notes,
            ComputeStatus(staff),
            staff.CreatedAt,
            staff.UpdatedAt);

    private static SessionListItemResponse MapSession(UsbSession session)
        => new(
            session.Id,
            session.SessionId,
            session.Host.HostName,
            session.DeviceSerial,
            session.Vendor,
            session.Model,
            session.ConnectedAt,
            session.DisconnectedAt,
            session.TotalBytesWritten,
            session.RiskScore,
            session.DetectionComplete);
}
