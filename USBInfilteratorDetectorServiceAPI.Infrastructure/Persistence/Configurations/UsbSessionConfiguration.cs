using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class UsbSessionConfiguration : IEntityTypeConfiguration<UsbSession>
{
    public void Configure(EntityTypeBuilder<UsbSession> builder)
    {
        builder.ToTable("UsbSessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceSerial).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Vendor).HasMaxLength(128);
        builder.Property(x => x.Model).HasMaxLength(128);
        builder.HasIndex(x => new { x.OrganizationId, x.SessionId }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.RiskScore });
        builder.HasIndex(x => x.DeviceSerial);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.UsbSessions)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Host)
            .WithMany(x => x.UsbSessions)
            .HasForeignKey(x => x.HostId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
