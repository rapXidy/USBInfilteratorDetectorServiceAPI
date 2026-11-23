using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Configurations;

public class RawEventConfiguration : IEntityTypeConfiguration<RawEvent>
{
    public void Configure(EntityTypeBuilder<RawEvent> builder)
    {
        builder.ToTable("RawEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Activity).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Description).HasMaxLength(1024);

        builder.HasOne(x => x.UsbSession)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.UsbSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
