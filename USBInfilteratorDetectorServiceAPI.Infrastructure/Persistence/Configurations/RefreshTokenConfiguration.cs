using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(512);
        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasOne(x => x.Auditor)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.AuditorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
