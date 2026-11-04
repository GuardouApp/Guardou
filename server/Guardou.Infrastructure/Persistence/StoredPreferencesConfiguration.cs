using Guardou.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guardou.Infrastructure.Persistence;

internal sealed class StoredPreferencesConfiguration : IEntityTypeConfiguration<StoredPreferences>
{
    public void Configure(EntityTypeBuilder<StoredPreferences> builder)
    {
        builder.ToTable("UserPreferences");
        builder.HasKey(preferences => preferences.UserId);
        builder.HasOne<ApplicationUser>().WithOne().HasForeignKey<StoredPreferences>(preferences => preferences.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
