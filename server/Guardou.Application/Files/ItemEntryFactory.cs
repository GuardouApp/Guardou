using Guardou.Application.Previews;
using Guardou.Domain.Files;

namespace Guardou.Application.Files;

internal sealed class ItemEntryFactory(IMimeTypeMap mimeTypes, IPreviewRenderer previews)
{
    public ItemEntry Create(Item item)
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
            HasPreview(item),
            HasPreview(item) && item.PreviewId is { } previewId ? ItemId.Format(previewId) : null,
            new ItemPermissions(CanModify: !item.IsRoot, CanAddChildren: item.IsFolder));
    }

    public IReadOnlyList<ItemEntry> Create(IEnumerable<Item> items)
    {
        return items
            .OrderBy(item => item.Kind == ItemKind.File)
            .ThenBy(item => item.Name, StringComparer.Ordinal)
            .Select(Create)
            .ToList();
    }

    public static Breadcrumb Breadcrumb(Item item)
    {
        return new Breadcrumb(ItemId.Format(item.Id), item.Name);
    }

    private bool HasPreview(Item item)
    {
        return !item.IsFolder && previews.Supports(item.Name);
    }
}
