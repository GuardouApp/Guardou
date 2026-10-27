using Guardou.Domain.Files;
using Guardou.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Guardou.Infrastructure.Persistence;

internal sealed class ItemOpenConfiguration : IEntityTypeConfiguration<ItemOpen>
{
    public void Configure(EntityTypeBuilder<ItemOpen> builder)
    {
        builder.ToTable("ItemOpens");
        builder.HasKey(open => new { open.UserId, open.ItemId });
        builder.Property(open => open.Opened).HasConversion<DateTimeOffsetToBinaryConverter>();

        builder.HasOne<Item>().WithMany().HasForeignKey(open => open.ItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(open => open.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(open => open.ItemId);
    }
}
