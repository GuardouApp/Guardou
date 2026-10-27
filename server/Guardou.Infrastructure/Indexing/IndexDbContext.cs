using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Guardou.Infrastructure.Indexing;

internal sealed class IndexDbContext(DbContextOptions<IndexDbContext> options) : DbContext(options)
{
    public const string NamesTable = "IndexNames";

    public DbSet<IndexEntry> Entries => Set<IndexEntry>();

    public DbSet<IndexName> Names => Set<IndexName>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IndexEntry>(entry =>
        {
            entry.ToTable("IndexEntries");
            entry.HasKey(item => item.Id);
            entry.Property(item => item.Type).HasConversion<string>();
            entry.Property(item => item.Modified).HasConversion<DateTimeOffsetToBinaryConverter>();
            entry.HasIndex(item => item.ItemId).IsUnique();
            entry.HasIndex(item => item.Path);
            entry.HasIndex(item => item.OwnerId);
        });
        modelBuilder.Entity<IndexName>(name =>
        {
            name.ToTable(NamesTable, table => table.ExcludeFromMigrations());
            name.HasKey(item => item.RowId);
            name.Property(item => item.RowId).HasColumnName("rowid");
        });
    }
}
