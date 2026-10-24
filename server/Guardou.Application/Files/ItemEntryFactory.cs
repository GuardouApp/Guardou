using Guardou.Application.Previews;
using Guardou.Application.Users;
using Guardou.Domain.Files;

namespace Guardou.Application.Files;

internal sealed class ItemEntryFactory(
    IMimeTypeMap mimeTypes,
    IPreviewRenderer previews,
    IUserNames users,
    IProfilePictureStore pictures)
{
    private static readonly ItemPermissions ReadOnly = new(CanModify: false, CanAddChildren: false);

    private readonly Dictionary<string, ItemCreator> _creators = [];

    public async Task<ItemEntry> CreateAsync(Item item, CancellationToken cancellationToken)
    {
        return (await CreateAsync([item], cancellationToken)).Single();
    }

    public async Task<IReadOnlyList<ItemEntry>> CreateAsync(IEnumerable<Item> items, CancellationToken cancellationToken)
    {
        List<Item> ordered = Ordered(items).ToList();
        await LoadCreatorsAsync(ordered, cancellationToken);
        return ordered.Select(Create).ToList();
    }

    public async Task<ItemEntry> CreateSharedAsync(Item item, Guid shareRootId, CancellationToken cancellationToken)
    {
        return (await CreateSharedAsync([item], shareRootId, cancellationToken)).Single();
    }

    public async Task<IReadOnlyList<ItemEntry>> CreateSharedAsync(
        IEnumerable<Item> items,
        Guid shareRootId,
        CancellationToken cancellationToken)
    {
        string rootId = ItemId.Format(shareRootId);
        return ReadOnlyEntries(await CreateAsync(items, cancellationToken), entry => entry.Id == rootId);
    }

    public async Task<IReadOnlyList<ItemEntry>> CreateSharedRootsAsync(IEnumerable<Item> items, CancellationToken cancellationToken)
    {
        return ReadOnlyEntries(await CreateAsync(items, cancellationToken), _ => true);
    }

    public Task<IReadOnlyList<ItemEntry>> CreateAsync(IEnumerable<Item> items, ItemAccess access, CancellationToken cancellationToken)
    {
        return access.ShareRootId is { } shareRootId
            ? CreateSharedAsync(items, shareRootId, cancellationToken)
            : CreateAsync(items, cancellationToken);
    }

    public static Breadcrumb Breadcrumb(Item item)
    {
        return new Breadcrumb(ItemId.Format(item.Id), item.Name);
    }

    private static List<ItemEntry> ReadOnlyEntries(IEnumerable<ItemEntry> entries, Func<ItemEntry, bool> isShareRoot)
    {
        return entries
            .Select(entry => entry with
            {
                ParentId = isShareRoot(entry) ? null : entry.ParentId,
                Permissions = ReadOnly,
            })
            .ToList();
    }

    private static IEnumerable<Item> Ordered(IEnumerable<Item> items)
    {
        return items
            .OrderBy(item => item.Kind == ItemKind.File)
            .ThenBy(item => item.Name, StringComparer.Ordinal);
    }

    private async Task LoadCreatorsAsync(IEnumerable<Item> items, CancellationToken cancellationToken)
    {
        List<string> missing = items.Select(item => item.CreatedBy).Distinct().Where(id => !_creators.ContainsKey(id)).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        foreach ((string id, string displayName) in await users.DisplayNamesAsync(missing, cancellationToken))
        {
            _creators[id] = new ItemCreator(id, displayName, pictures.Contains(id));
        }
    }

    private ItemEntry Create(Item item)
    {
        return new ItemEntry(
            ItemId.Format(item.Id),
            item.ParentId is { } parentId ? ItemId.Format(parentId) : null,
            item.Name,
            item.Kind,
            item.Size,
            item.IsFolder ? null : mimeTypes.MimeTypeOf(item.Name),
            item.Modified,
            item.Created,
            _creators[item.CreatedBy],
            HasPreview(item),
            new ItemPermissions(CanModify: !item.IsRoot, CanAddChildren: item.IsFolder));
    }

    private bool HasPreview(Item item)
    {
        return !item.IsFolder && previews.Supports(item.Name);
    }
}
