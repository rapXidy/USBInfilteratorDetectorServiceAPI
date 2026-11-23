using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CorpCode).HasMaxLength(64);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Phone).HasMaxLength(64);
        builder.Property(x => x.OfficeAddress).HasMaxLength(512);
        builder.Property(x => x.Sector).HasMaxLength(128);
        builder.Property(x => x.OnboardingStatus).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(x => x.CorpCode)
            .IsUnique()
            .HasFilter("[CorpCode] IS NOT NULL");
    }
}
