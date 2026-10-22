using Guardou.Application.Files;
using Guardou.Application.Settings;
using Guardou.Domain.Files;
using Guardou.Domain.Shares;

namespace Guardou.Application.Shares;

public sealed class ItemSharing
{
    private const string LinkPath = "/s/";

    private readonly IShareStore _shares;
    private readonly OwnedItems _items;
    private readonly ServerSettingsAdministration _server;
    private readonly TimeProvider _time;

    internal ItemSharing(IShareStore shares, OwnedItems items, ServerSettingsAdministration server, TimeProvider time)
    {
        _shares = shares;
        _items = items;
        _server = server;
        _time = time;
    }

    public async Task<ShareLink> GetAsync(string ownerId, string id, string requestOrigin, CancellationToken cancellationToken)
    {
        Item file = await FindFileAsync(ownerId, id, cancellationToken);
        Share share = await _shares.FindByItemAsync(file.Id, cancellationToken)
            ?? throw new ShareNotFoundException($"The item '{id}' is not shared.");
        return await LinkOfAsync(share, requestOrigin, cancellationToken);
    }

    public async Task<ShareLink> ShareAsync(string ownerId, string id, string requestOrigin, CancellationToken cancellationToken)
    {
        Item file = await FindFileAsync(ownerId, id, cancellationToken);
        Share? share = await _shares.FindByItemAsync(file.Id, cancellationToken);
        if (share is null)
        {
            share = Share.Create(file.Id, _time.GetUtcNow());
            _shares.Add(share);
            await _shares.SaveAsync(cancellationToken);
        }

        return await LinkOfAsync(share, requestOrigin, cancellationToken);
    }

    public async Task StopSharingAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        Item file = await FindFileAsync(ownerId, id, cancellationToken);
        if (await _shares.FindByItemAsync(file.Id, cancellationToken) is { } share)
        {
            _shares.Remove(share);
            await _shares.SaveAsync(cancellationToken);
        }
    }

    private async Task<Item> FindFileAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        Item item = (await _items.FindActiveAsync(ownerId, id, cancellationToken)).Item;
        return item.IsFolder ? throw new InvalidItemRequestException("Only files can be shared.") : item;
    }

    private async Task<ShareLink> LinkOfAsync(Share share, string requestOrigin, CancellationToken cancellationToken)
    {
        string origin = await _server.PublicOriginAsync(requestOrigin, cancellationToken);
        return new ShareLink(origin + LinkPath + share.Token);
    }
}
