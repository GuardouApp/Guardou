namespace Guardou.Application.Search;

public sealed record SearchScope(IReadOnlyCollection<Guid> Subtrees, Guid Excluded, string ViewerId);
