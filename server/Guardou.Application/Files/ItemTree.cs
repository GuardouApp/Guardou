using Guardou.Domain.Files;

namespace Guardou.Application.Files;

internal sealed class ItemTree(IItemStore store)
{
    public async Task<IReadOnlyList<Item>> SubtreeAsync(Item item, CancellationToken cancellationToken)
    {
        List<Item> items = [item];
        for (int index = 0; index < items.Count; index++)
        {
            if (items[index].IsFolder)
            {
                items.AddRange(await store.ChildrenWithTrashAsync(items[index].Id, cancellationToken));
            }
        }

        return items;
    }

    public async Task<IReadOnlyList<TreeNode>> ActiveSubtreeAsync(Item item, string path, CancellationToken cancellationToken)
    {
        List<TreeNode> nodes = [new TreeNode(item, path)];
        for (int index = 0; index < nodes.Count; index++)
        {
            TreeNode node = nodes[index];
            if (node.Item.IsFolder)
            {
                IReadOnlyList<Item> children = await store.ChildrenAsync(node.Item.Id, cancellationToken);
                nodes.AddRange(children.Select(child => new TreeNode(child, $"{node.Path}/{child.Name}")));
            }
        }

        return nodes;
    }
}

internal sealed record TreeNode(Item Item, string Path);
