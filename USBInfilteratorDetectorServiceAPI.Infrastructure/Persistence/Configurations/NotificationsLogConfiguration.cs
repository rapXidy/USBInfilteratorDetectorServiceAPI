using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class NotificationsLogConfiguration : IEntityTypeConfiguration<NotificationsLog>
{
    public void Configure(EntityTypeBuilder<NotificationsLog> builder)
    {
        builder.ToTable("NotificationsLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Severity).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1024);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
