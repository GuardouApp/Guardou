using Guardou.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guardou.Infrastructure.Persistence;

internal sealed class TaskQueueSettingsConfiguration : IEntityTypeConfiguration<TaskQueueSettings>
{
    public void Configure(EntityTypeBuilder<TaskQueueSettings> builder)
    {
        builder.ToTable("TaskQueueSettings");
        builder.HasKey(settings => settings.Kind);
        builder.Property(settings => settings.Kind).HasConversion<string>();
    }
}
