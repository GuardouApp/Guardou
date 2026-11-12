using Guardou.Domain.Files;

namespace Guardou.Application.Files;

internal sealed class ItemTree(IItemStore store)
{
    public Task<IReadOnlyList<Item>> SubtreeAsync(Item item, CancellationToken cancellationToken)
    {
        return store.SubtreeAsync(item.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<TreeNode>> ActiveSubtreeAsync(Item item, string path, CancellationToken cancellationToken)
    {
        Dictionary<Guid, string> paths = [];
        Dictionary<Guid, CaseInsensitiveNames> names = [];
        List<TreeNode> nodes = [];
        foreach (Item current in await store.ActiveSubtreeAsync(item.Id, cancellationToken))
        {
            string currentPath = current.Id == item.Id || current.ParentId is not { } parentId
                ? path
                : $"{paths[parentId]}/{NamesIn(names, parentId).Claim(current.Name)}";
            paths[current.Id] = currentPath;
            nodes.Add(new TreeNode(current, currentPath));
        }

        return nodes;
    }

    private static CaseInsensitiveNames NamesIn(Dictionary<Guid, CaseInsensitiveNames> names, Guid parentId)
    {
        if (!names.TryGetValue(parentId, out CaseInsensitiveNames? folder))
        {
            folder = new CaseInsensitiveNames();
            names[parentId] = folder;
        }

        return folder;
    }
}

internal sealed record TreeNode(Item Item, string Path);
