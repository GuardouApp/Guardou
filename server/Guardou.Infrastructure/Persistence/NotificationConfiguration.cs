using Guardou.Domain.Files;
using Guardou.Domain.Notifications;
using Guardou.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Guardou.Infrastructure.Persistence;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    private const int VersionLength = 64;

    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.Created).HasConversion<DateTimeOffsetToBinaryConverter>();
        builder.Property(notification => notification.Kind).HasConversion<string>();
        builder.Property(notification => notification.Version).HasMaxLength(VersionLength);

        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(notification => notification.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(notification => notification.SenderId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Item>().WithMany().HasForeignKey(notification => notification.ItemId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(notification => new { notification.UserId, notification.Created });
        builder.HasIndex(notification => notification.SenderId);
        builder.HasIndex(notification => notification.ItemId);
    }
}
