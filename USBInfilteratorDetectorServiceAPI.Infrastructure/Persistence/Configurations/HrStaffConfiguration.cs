using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class HrStaffConfiguration : IEntityTypeConfiguration<HrStaff>
{
    public void Configure(EntityTypeBuilder<HrStaff> builder)
    {
        builder.ToTable("HrStaff");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Department).IsRequired().HasMaxLength(128);
        builder.Property(x => x.PhoneNumber).HasMaxLength(64);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.JobTitle).HasMaxLength(128);
        builder.Property(x => x.EmployeeNumber).HasMaxLength(64);
        builder.Property(x => x.WindowsUsername).HasMaxLength(256);
        builder.Property(x => x.DeviceId).HasMaxLength(128);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasIndex(x => new { x.OrganizationId, x.EmployeeNumber })
            .IsUnique()
            .HasFilter("[EmployeeNumber] IS NOT NULL");
        builder.HasIndex(x => new { x.OrganizationId, x.WindowsUsername });
        builder.HasIndex(x => new { x.OrganizationId, x.DeviceId });
        builder.HasIndex(x => new { x.OrganizationId, x.DateOfResignation });

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.HrStaffMembers)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
