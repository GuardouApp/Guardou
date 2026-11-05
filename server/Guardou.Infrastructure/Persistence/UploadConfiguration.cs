using Guardou.Domain.Files;
using Guardou.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Guardou.Infrastructure.Persistence;

internal sealed class UploadConfiguration : IEntityTypeConfiguration<Upload>
{
    public void Configure(EntityTypeBuilder<Upload> builder)
    {
        builder.ToTable("Uploads");
        builder.HasKey(upload => upload.Id);
        builder.Ignore(upload => upload.Destination);
        builder.Property(upload => upload.Name).HasMaxLength(ItemName.MaximumLength);
        builder.Property(upload => upload.Modified).HasConversion<DateTimeOffsetToBinaryConverter>();
        builder.Property(upload => upload.LastWrite).HasConversion<DateTimeOffsetToBinaryConverter>();

        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(upload => upload.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Item>().WithMany().HasForeignKey(upload => upload.FolderId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(upload => new { upload.FolderId, upload.Name });
        builder.HasIndex(upload => upload.LastWrite);
    }
}
