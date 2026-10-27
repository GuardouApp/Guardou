namespace Guardou.Infrastructure.Indexing;

internal sealed class IndexName
{
    public long RowId { get; private set; }

    public string Name { get; private set; } = string.Empty;
}
