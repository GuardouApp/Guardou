using Guardou.Application.Shares;
using Guardou.Domain.Files;

namespace Guardou.Application.Files;

internal sealed class AccessibleItems(IItemStore store, OwnedItems owned, IDirectShareStore shares)
{
    public async Task<ItemAccess> FindActiveAsync(string userId, string reference, CancellationToken cancellationToken)
    {
        if (reference == OwnedItems.RootAlias)
        {
            return ItemAccess.Owned(await owned.FindActiveAsync(userId, reference, cancellationToken), userId);
        }

        return await TryFindActiveAsync(userId, OwnedItems.ParseId(reference), cancellationToken)
            ?? throw OwnedItems.NotFound(reference);
    }

    public async Task<ItemAccess?> TryFindActiveAsync(string userId, Guid id, CancellationToken cancellationToken)
    {
        ItemPath? path = await owned.TryFindActiveAsync(userId, id, cancellationToken);
        return path is not null ? ItemAccess.Owned(path, userId) : await TryFindSharedAsync(userId, id, cancellationToken);
    }

    public async Task<ItemAccess> FindActiveFolderAsync(string userId, string reference, CancellationToken cancellationToken)
    {
        ItemAccess folder = await FindActiveAsync(userId, reference, cancellationToken);
        return folder.Item.IsFolder ? folder : throw OwnedItems.NotAFolder();
    }

    private async Task<ItemAccess?> TryFindSharedAsync(string userId, Guid id, CancellationToken cancellationToken)
    {
        Item? item = await store.FindAnyAsync(id, cancellationToken);
        ItemPath? path = item is null ? null : await owned.TryFindActiveAsync(item.OwnerId, id, cancellationToken);
        if (path is null)
        {
            return null;
        }

        HashSet<Guid> shared = (await shares.OfUserAsync(userId, cancellationToken)).Select(share => share.ItemId).ToHashSet();
        Item? root = path.Ancestors.Append(path.Item).FirstOrDefault(candidate => shared.Contains(candidate.Id));
        return root is null ? null : new ItemAccess(path, root.Id, userId);
    }
}
