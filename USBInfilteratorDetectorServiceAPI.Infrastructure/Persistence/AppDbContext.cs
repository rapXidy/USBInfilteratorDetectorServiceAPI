using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the USB Infilterator Detector API.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationApiCredential> OrganizationApiCredentials => Set<OrganizationApiCredential>();
    public DbSet<Auditor> Auditors => Set<Auditor>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AllowedDevice> AllowedDevices => Set<AllowedDevice>();
    public DbSet<HrStaff> HrStaff => Set<HrStaff>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<Host> Hosts => Set<Host>();
    public DbSet<UsbSession> UsbSessions => Set<UsbSession>();
    public DbSet<SessionReason> SessionReasons => Set<SessionReason>();
    public DbSet<RawEvent> RawEvents => Set<RawEvent>();
    public DbSet<NotificationsLog> NotificationsLogs => Set<NotificationsLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
