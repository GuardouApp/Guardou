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
    private readonly ItemDownloads _downloads;
    private readonly ItemPreviews _previews;
    private readonly IProfilePictureStore _pictures;

    internal SharedItems(
        IShareStore shares,
        IItemStore store,
        OwnedItems items,
        ItemEntryFactory entries,
        ItemDownloads downloads,
        ItemPreviews previews,
        IProfilePictureStore pictures)
    {
        _shares = shares;
        _store = store;
        _items = items;
        _entries = entries;
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
        SharedPath shared = await FindAsync(token, id, cancellationToken);
        Item item = shared.Path.Item;
        IReadOnlyList<Breadcrumb> ancestors = shared.Path.Ancestors
            .SkipWhile(ancestor => ancestor.Id != shared.Root.Id)
            .Select(ItemEntryFactory.Breadcrumb)
            .ToList();
        IReadOnlyList<ItemEntry>? children = item.IsFolder
            ? await _entries.CreateSharedAsync(await _store.ChildrenAsync(item.Id, cancellationToken), shared.Root.Id, cancellationToken)
            : null;
        ItemEntry entry = await _entries.CreateSharedAsync(item, shared.Root.Id, cancellationToken);
        return new ItemDetails(entry, ancestors, children, Content: null);
    }

    public async Task<FileDownload?> TryOpenFileAsync(string token, string id, CancellationToken cancellationToken)
    {
        SharedPath shared = await FindAsync(token, id, cancellationToken);
        return await _downloads.TryOpenFileAsync(shared.Root.OwnerId, id, cancellationToken);
    }

    public async Task<FileDownload> OpenFileAsync(string token, string id, CancellationToken cancellationToken)
    {
        SharedPath shared = await FindAsync(token, id, cancellationToken);
        return await _downloads.OpenFileAsync(shared.Root.OwnerId, id, cancellationToken);
    }

    public async Task<ItemArchive> PrepareArchiveAsync(string token, string id, CancellationToken cancellationToken)
    {
        SharedPath shared = await FindAsync(token, id, cancellationToken);
        return await _downloads.PrepareArchiveAsync(shared.Root.OwnerId, [id], cancellationToken);
    }

    public async Task<Stream?> OpenPreviewAsync(string token, string id, PreviewSize size, CancellationToken cancellationToken)
    {
        SharedPath shared = await FindAsync(token, id, cancellationToken);
        return await _previews.OpenAsync(shared.Root.OwnerId, id, size, cancellationToken);
    }

    public async Task<ProfilePicture?> OpenCreatorPictureAsync(string token, string id, CancellationToken cancellationToken)
    {
        SharedPath shared = await FindAsync(token, id, cancellationToken);
        return _pictures.TryOpen(shared.Path.Item.CreatedBy);
    }

    private static ShareNotFoundException NotFound()
    {
        return new ShareNotFoundException("The share does not exist.");
    }

    private async Task<SharedPath> FindAsync(string token, string id, CancellationToken cancellationToken)
    {
        Item root = await FindRootAsync(token, cancellationToken);
        ItemPath? path = ItemId.TryParse(id, out Guid itemId)
            ? await _items.TryFindActiveAsync(root.OwnerId, itemId, cancellationToken)
            : null;
        return path is not null && path.Contains(root.Id) ? new SharedPath(root, path) : throw NotFound();
    }

    private async Task<Item> FindRootAsync(string token, CancellationToken cancellationToken)
    {
        Share share = (ShareToken.IsValid(token) ? await _shares.FindByTokenAsync(token, cancellationToken) : null) ?? throw NotFound();
        Item item = await _store.FindAnyAsync(share.ItemId, cancellationToken) ?? throw NotFound();
        ItemPath? path = await _items.TryFindActiveAsync(item.OwnerId, item.Id, cancellationToken);
        return path?.Item ?? throw NotFound();
    }

    private sealed record SharedPath(Item Root, ItemPath Path);
}
