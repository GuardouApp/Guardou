using Guardou.Domain.Files;

namespace Guardou.Infrastructure.Indexing;

internal sealed class IndexEntry
{
    public long Id { get; private set; }

    public required Guid ItemId { get; init; }

    public required string OwnerId { get; init; }

    public required Guid? ParentId { get; init; }

    public required string Path { get; init; }

    public required string Name { get; set; }

    public required ItemType Type { get; set; }

    public required long Size { get; init; }

    public required DateTimeOffset Modified { get; set; }
}
