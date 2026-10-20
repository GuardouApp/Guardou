using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guardou.Infrastructure.Persistence;

internal sealed class StoredSettingConfiguration : IEntityTypeConfiguration<StoredSetting>
{
    public void Configure(EntityTypeBuilder<StoredSetting> builder)
    {
        builder.ToTable("Settings");
        builder.HasKey(setting => setting.Key);
    }
}
