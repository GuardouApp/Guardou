using Guardou.Application.Previews;
using Guardou.Domain.Files;

namespace Guardou.Application.Files;

internal sealed class ItemRemover(IItemStore store, IBlobStore blobs, IPreviewCache previews, ItemTree tree)
{
    public async Task RemoveAsync(IEnumerable<Item> items, CancellationToken cancellationToken)
    {
        List<Item> files = [];
        foreach (Item item in items)
        {
            foreach (Item removed in (await tree.SubtreeAsync(item, cancellationToken)).Reverse())
            {
                store.Remove(removed);
                if (!removed.IsFolder)
                {
                    files.Add(removed);
                }
            }
        }

        await store.SaveAsync(cancellationToken);
        foreach (Item file in files.DistinctBy(file => file.Id))
        {
            if (file.StorageKey is { } key)
            {
                blobs.Delete(key);
            }

            previews.Delete(file.OwnerId, file.Id);
        }
    }
}
