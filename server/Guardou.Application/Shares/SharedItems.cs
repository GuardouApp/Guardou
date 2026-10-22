using Guardou.Application.Files;
using Guardou.Application.Previews;
using Guardou.Domain.Files;
using Guardou.Domain.Previews;
using Guardou.Domain.Shares;

namespace Guardou.Application.Shares;

public sealed class SharedItems
{
    private readonly IShareStore _shares;
    private readonly IItemStore _store;
    private readonly OwnedItems _items;
    private readonly ItemEntryFactory _entries;
    private readonly ItemDownloads _downloads;
    private readonly ItemPreviews _previews;

    internal SharedItems(
        IShareStore shares,
        IItemStore store,
        OwnedItems items,
        ItemEntryFactory entries,
        ItemDownloads downloads,
        ItemPreviews previews)
    {
        _shares = shares;
        _store = store;
        _items = items;
        _entries = entries;
        _downloads = downloads;
        _previews = previews;
    }

    public async Task<ItemEntry> GetAsync(string token, CancellationToken cancellationToken)
    {
        return _entries.CreateShared(await FindRootAsync(token, cancellationToken));
    }

    public async Task<FileDownload> OpenFileAsync(string token, string id, CancellationToken cancellationToken)
    {
        Item item = await FindAsync(token, id, cancellationToken);
        return await _downloads.OpenFileAsync(item.OwnerId, id, cancellationToken);
    }

    public async Task<Stream?> OpenPreviewAsync(string token, string id, PreviewSize size, CancellationToken cancellationToken)
    {
        Item item = await FindAsync(token, id, cancellationToken);
        return await _previews.OpenAsync(item.OwnerId, id, size, cancellationToken);
    }

    private static ShareNotFoundException NotFound()
    {
        return new ShareNotFoundException("The share does not exist.");
    }

    private async Task<Item> FindAsync(string token, string id, CancellationToken cancellationToken)
    {
        Item item = await FindRootAsync(token, cancellationToken);
        return ItemId.Format(item.Id) == id ? item : throw NotFound();
    }

    private async Task<Item> FindRootAsync(string token, CancellationToken cancellationToken)
    {
        Share share = (ShareToken.IsValid(token) ? await _shares.FindByTokenAsync(token, cancellationToken) : null) ?? throw NotFound();
        Item item = await _store.FindAnyAsync(share.ItemId, cancellationToken) ?? throw NotFound();
        ItemPath? path = await _items.TryFindActiveAsync(item.OwnerId, item.Id, cancellationToken);
        return path?.Item ?? throw NotFound();
    }
}
