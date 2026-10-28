using Guardou.Domain.Files;

namespace Guardou.Application.Search;

public sealed record SearchQuery(
    IReadOnlyList<string> Terms,
    IReadOnlySet<ItemType> Types,
    DateTimeOffset? ModifiedFrom,
    DateTimeOffset? ModifiedBefore,
    long? LargerThan,
    long? SmallerThan,
    bool Starred)
{
    public bool IsEmpty => Terms.Count == 0
        && Types.Count == 0
        && ModifiedFrom is null
        && ModifiedBefore is null
        && LargerThan is null
        && SmallerThan is null
        && !Starred;
}
