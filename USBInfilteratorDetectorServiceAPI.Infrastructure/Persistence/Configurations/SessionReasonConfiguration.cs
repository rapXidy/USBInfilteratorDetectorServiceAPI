using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class SessionReasonConfiguration : IEntityTypeConfiguration<SessionReason>
{
    public void Configure(EntityTypeBuilder<SessionReason> builder)
    {
        builder.ToTable("SessionReasons");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RuleName).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(1024);
        builder.Property(x => x.EventRecordIdsJson).IsRequired();

        builder.HasOne(x => x.UsbSession)
            .WithMany(x => x.Reasons)
            .HasForeignKey(x => x.UsbSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
