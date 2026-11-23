using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Services.Devices;

/// <summary>
/// Manages the organization allowed-device list.
/// </summary>
public class AllowedDeviceService
{
    private readonly IAllowedDeviceRepository _devices;
    private readonly IUnitOfWork _unitOfWork;

    public AllowedDeviceService(IAllowedDeviceRepository devices, IUnitOfWork unitOfWork)
    {
        _devices = devices;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Lists allowed devices for the organization.
    /// </summary>
    public async Task<IReadOnlyList<AllowedDeviceResponse>> ListAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var devices = await _devices.ListByOrganizationAsync(organizationId, includeInactive: true, cancellationToken);
        return devices.Select(Map).ToList();
    }

    /// <summary>
    /// Adds an allowed device.
    /// </summary>
    public async Task<AllowedDeviceResponse> CreateAsync(Guid organizationId, Guid? auditorId, CreateAllowedDeviceRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceSerial))
            throw new ServiceException("DeviceSerial is required.");

        var device = new AllowedDevice
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            DeviceSerial = request.DeviceSerial.Trim(),
            Vendor = request.Vendor?.Trim(),
            Model = request.Model?.Trim(),
            Description = request.Description?.Trim(),
            AddedByAuditorId = auditorId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _devices.AddAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(device);
    }

    /// <summary>
    /// Updates an allowed device.
    /// </summary>
    public async Task<AllowedDeviceResponse> UpdateAsync(Guid organizationId, Guid deviceId, UpdateAllowedDeviceRequest request, CancellationToken cancellationToken = default)
    {
        var device = await _devices.GetByIdAsync(organizationId, deviceId, cancellationToken)
            ?? throw new ServiceException("Device not found.", 404);

        device.Vendor = request.Vendor?.Trim();
        device.Model = request.Model?.Trim();
        device.Description = request.Description?.Trim();
        device.IsActive = request.IsActive;
        _devices.Update(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(device);
    }

    /// <summary>
    /// Soft-deletes an allowed device.
    /// </summary>
    public async Task DeleteAsync(Guid organizationId, Guid deviceId, CancellationToken cancellationToken = default)
    {
        var device = await _devices.GetByIdAsync(organizationId, deviceId, cancellationToken)
            ?? throw new ServiceException("Device not found.", 404);

        device.IsActive = false;
        _devices.Update(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AllowedDeviceResponse Map(AllowedDevice device)
        => new(device.Id, device.DeviceSerial, device.Vendor, device.Model, device.Description, device.IsActive, device.CreatedAt);
}
