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
        List<TreeNode> nodes = [];
        foreach (Item current in await store.ActiveSubtreeAsync(item.Id, cancellationToken))
        {
            string currentPath = current.Id == item.Id || current.ParentId is not { } parentId
                ? path
                : $"{paths[parentId]}/{current.Name}";
            paths[current.Id] = currentPath;
            nodes.Add(new TreeNode(current, currentPath));
        }

        return nodes;
    }
}

internal sealed record TreeNode(Item Item, string Path);
