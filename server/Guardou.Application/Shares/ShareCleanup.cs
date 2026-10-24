using Guardou.Application.Files;
using Guardou.Domain.Shares;

namespace Guardou.Application.Shares;

internal sealed class ShareCleanup(IShareStore links, IDirectShareStore directShares, OwnedItems items)
{
    public async Task RemoveInactiveAsync(string ownerId, CancellationToken cancellationToken)
    {
        foreach (Share share in await links.OwnedAsync(ownerId, cancellationToken))
        {
            if (!await IsActiveAsync(ownerId, share.ItemId, cancellationToken))
            {
                links.Remove(share);
            }
        }

        foreach (DirectShare share in await directShares.OwnedAsync(ownerId, cancellationToken))
        {
            if (!await IsActiveAsync(ownerId, share.ItemId, cancellationToken))
            {
                directShares.Remove(share);
            }
        }

        await links.SaveAsync(cancellationToken);
        await directShares.SaveAsync(cancellationToken);
    }

    private async Task<bool> IsActiveAsync(string ownerId, Guid itemId, CancellationToken cancellationToken)
    {
        return await items.TryFindActiveAsync(ownerId, itemId, cancellationToken) is not null;
    }
}
