using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class AllowedDeviceConfiguration : IEntityTypeConfiguration<AllowedDevice>
{
    public void Configure(EntityTypeBuilder<AllowedDevice> builder)
    {
        builder.ToTable("AllowedDevices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceSerial).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Vendor).HasMaxLength(128);
        builder.Property(x => x.Model).HasMaxLength(128);
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.HasIndex(x => new { x.OrganizationId, x.DeviceSerial });

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.AllowedDevices)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
