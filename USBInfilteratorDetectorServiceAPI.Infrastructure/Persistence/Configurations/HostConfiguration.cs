using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class HostConfiguration : IEntityTypeConfiguration<Host>
{
    public void Configure(EntityTypeBuilder<Host> builder)
    {
        builder.ToTable("Hosts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.HostName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.DeviceId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.OsVersion).HasMaxLength(128);
        builder.Property(x => x.LoggedInUser).HasMaxLength(256);
        builder.HasIndex(x => new { x.OrganizationId, x.DeviceId }).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Hosts)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
