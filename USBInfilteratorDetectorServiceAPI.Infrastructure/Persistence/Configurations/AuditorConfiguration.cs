using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class AuditorConfiguration : IEntityTypeConfiguration<Auditor>
{
    public void Configure(EntityTypeBuilder<Auditor> builder)
    {
        builder.ToTable("Auditors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).IsRequired().HasMaxLength(128);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(x => new { x.OrganizationId, x.Username }).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Auditors)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
