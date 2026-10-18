using Guardou.Domain.Files;

namespace Guardou.Application.Files;

internal sealed record ItemPath(Item Item, IReadOnlyList<Item> Ancestors)
{
    public bool Contains(Guid id)
    {
        return Item.Id == id || Ancestors.Any(ancestor => ancestor.Id == id);
    }
}
