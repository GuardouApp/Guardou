using Guardou.Domain.Files;

namespace Guardou.Application.Files;

internal sealed class ItemRemover(IItemStore store, IBlobStore blobs, ItemTree tree)
{
    public async Task RemoveAsync(IEnumerable<Item> items, CancellationToken cancellationToken)
    {
        List<string> keys = [];
        foreach (Item item in items)
        {
            foreach (Item removed in (await tree.SubtreeAsync(item, cancellationToken)).Reverse())
            {
                store.Remove(removed);
                if (removed.StorageKey is { } key)
                {
                    keys.Add(key);
                }
            }
        }

        await store.SaveAsync(cancellationToken);
        foreach (string key in keys.Distinct())
        {
            blobs.Delete(key);
        }
    }
}
