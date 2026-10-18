using Guardou.Domain.Files;
using Guardou.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Guardou.Infrastructure.Persistence;

internal sealed class QueuedTaskConfiguration : IEntityTypeConfiguration<QueuedTask>
{
    public void Configure(EntityTypeBuilder<QueuedTask> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Kind).HasConversion<string>();
        builder.Property(task => task.State).HasConversion<string>();
        builder.Property(task => task.Error).HasMaxLength(QueuedTask.MaximumErrorLength);
        builder.Property(task => task.Created).HasConversion<DateTimeOffsetToBinaryConverter>();
        builder.Property(task => task.Updated).HasConversion<DateTimeOffsetToBinaryConverter>();

        builder.HasOne<Item>().WithMany().HasForeignKey(task => task.ItemId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(task => new { task.Kind, task.ItemId }).IsUnique();
        builder.HasIndex(task => new { task.State, task.Priority, task.Created });
    }
}
