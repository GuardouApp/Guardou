using Guardou.Application.Files;
using Guardou.Application.Previews;
using Guardou.Application.Users;
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
    private readonly ItemBrowser _browser;
    private readonly ItemDownloads _downloads;
    private readonly ItemPreviews _previews;
    private readonly IProfilePictureStore _pictures;

    internal SharedItems(
        IShareStore shares,
        IItemStore store,
        OwnedItems items,
        ItemEntryFactory entries,
        ItemBrowser browser,
        ItemDownloads downloads,
        ItemPreviews previews,
        IProfilePictureStore pictures)
    {
        _shares = shares;
        _store = store;
        _items = items;
        _entries = entries;
        _browser = browser;
        _downloads = downloads;
        _previews = previews;
        _pictures = pictures;
    }

    public async Task<ItemEntry> GetAsync(string token, CancellationToken cancellationToken)
    {
        Item root = await FindRootAsync(token, cancellationToken);
        return await _entries.CreateSharedAsync(root, root.Id, cancellationToken);
    }

    public async Task<ItemDetails> GetDetailsAsync(string token, string id, CancellationToken cancellationToken)
    {
        return await _browser.DetailsAsync(await FindAsync(token, id, cancellationToken), content: false, cancellationToken);
    }

    public async Task<FileDownload?> TryOpenFileAsync(string token, string id, CancellationToken cancellationToken)
    {
        ItemAccess shared = await FindAsync(token, id, cancellationToken);
        return await _downloads.TryOpenFileAsync(shared.Item.OwnerId, id, cancellationToken);
    }

    public async Task<FileDownload> OpenFileAsync(string token, string id, CancellationToken cancellationToken)
    {
        ItemAccess shared = await FindAsync(token, id, cancellationToken);
        return await _downloads.OpenFileAsync(shared.Item.OwnerId, id, cancellationToken);
    }

    public async Task<ItemArchive> PrepareArchiveAsync(string token, string id, CancellationToken cancellationToken)
    {
        ItemAccess shared = await FindAsync(token, id, cancellationToken);
        return await _downloads.PrepareArchiveAsync(shared.Item.OwnerId, [id], cancellationToken);
    }

    public async Task<Stream?> OpenPreviewAsync(string token, string id, PreviewSize size, CancellationToken cancellationToken)
    {
        ItemAccess shared = await FindAsync(token, id, cancellationToken);
        return await _previews.OpenAsync(shared.Item.OwnerId, id, size, cancellationToken);
    }

    public async Task<ProfilePicture?> OpenCreatorPictureAsync(string token, string id, CancellationToken cancellationToken)
    {
        ItemAccess shared = await FindAsync(token, id, cancellationToken);
        return _pictures.TryOpen(shared.Item.CreatedBy);
    }

    private static ShareNotFoundException NotFound()
    {
        return new ShareNotFoundException("The share does not exist.");
    }

    private async Task<ItemAccess> FindAsync(string token, string id, CancellationToken cancellationToken)
    {
        Item root = await FindRootAsync(token, cancellationToken);
        ItemPath? path = ItemId.TryParse(id, out Guid itemId)
            ? await _items.TryFindActiveAsync(root.OwnerId, itemId, cancellationToken)
            : null;
        return path is not null && path.Contains(root.Id) ? new ItemAccess(path, root.Id) : throw NotFound();
    }

    private async Task<Item> FindRootAsync(string token, CancellationToken cancellationToken)
    {
        Share share = (ShareToken.IsValid(token) ? await _shares.FindByTokenAsync(token, cancellationToken) : null) ?? throw NotFound();
        Item item = await _store.FindAnyAsync(share.ItemId, cancellationToken) ?? throw NotFound();
        ItemPath? path = await _items.TryFindActiveAsync(item.OwnerId, item.Id, cancellationToken);
        return path?.Item ?? throw NotFound();
    }
}
