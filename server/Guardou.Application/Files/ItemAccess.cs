using Guardou.Domain.Files;

namespace Guardou.Application.Files;

internal sealed record ItemAccess(ItemPath Path, Guid? ShareRootId)
{
    public Item Item => Path.Item;

    public IReadOnlyList<Breadcrumb> Breadcrumbs => VisibleAncestors().Select(ItemEntryFactory.Breadcrumb).ToList();

    public static ItemAccess Owned(ItemPath path)
    {
        return new ItemAccess(path, ShareRootId: null);
    }

    private IEnumerable<Item> VisibleAncestors()
    {
        return ShareRootId is { } rootId ? Path.Ancestors.SkipWhile(ancestor => ancestor.Id != rootId) : Path.Ancestors;
    }
}
