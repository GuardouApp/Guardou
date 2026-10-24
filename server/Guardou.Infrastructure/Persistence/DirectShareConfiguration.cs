using Guardou.Domain.Files;
using Guardou.Domain.Shares;
using Guardou.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Guardou.Infrastructure.Persistence;

internal sealed class DirectShareConfiguration : IEntityTypeConfiguration<DirectShare>
{
    public void Configure(EntityTypeBuilder<DirectShare> builder)
    {
        builder.ToTable("DirectShares");
        builder.HasKey(share => share.Id);
        builder.Property(share => share.Created).HasConversion<DateTimeOffsetToBinaryConverter>();

        builder.HasOne<Item>().WithMany().HasForeignKey(share => share.ItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(share => share.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(share => new { share.ItemId, share.UserId }).IsUnique();
        builder.HasIndex(share => share.UserId);
    }
}
