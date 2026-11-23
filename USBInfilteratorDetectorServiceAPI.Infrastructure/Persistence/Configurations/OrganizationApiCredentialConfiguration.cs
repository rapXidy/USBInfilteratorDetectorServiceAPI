using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class OrganizationApiCredentialConfiguration : IEntityTypeConfiguration<OrganizationApiCredential>
{
    public void Configure(EntityTypeBuilder<OrganizationApiCredential> builder)
    {
        builder.ToTable("OrganizationApiCredentials");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ApiKeyHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.ApiKeyPrefix).IsRequired().HasMaxLength(32);
        builder.HasIndex(x => x.ApiKeyPrefix);
        builder.HasIndex(x => x.OrganizationId);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.ApiCredentials)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
