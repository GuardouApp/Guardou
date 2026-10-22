using Guardou.Application.Files;
using Guardou.Domain.Shares;

namespace Guardou.Application.Shares;

internal sealed class ShareCleanup(IShareStore shares, OwnedItems items)
{
    public async Task RemoveInactiveAsync(string ownerId, CancellationToken cancellationToken)
    {
        foreach (Share share in await shares.OwnedAsync(ownerId, cancellationToken))
        {
            if (await items.TryFindActiveAsync(ownerId, share.ItemId, cancellationToken) is null)
            {
                shares.Remove(share);
            }
        }

        await shares.SaveAsync(cancellationToken);
    }
}
