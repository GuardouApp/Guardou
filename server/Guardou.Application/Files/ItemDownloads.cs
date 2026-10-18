using System.IO.Compression;
using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public sealed class ItemDownloads
{
    private const string ArchiveExtension = ".zip";
    private const string DefaultArchiveName = "download";

    private readonly IBlobStore _blobs;
    private readonly IMimeTypeMap _mimeTypes;
    private readonly OwnedItems _items;
    private readonly ItemTree _tree;

    internal ItemDownloads(IBlobStore blobs, IMimeTypeMap mimeTypes, OwnedItems items, ItemTree tree)
    {
        _blobs = blobs;
        _mimeTypes = mimeTypes;
        _items = items;
        _tree = tree;
    }

    public async Task<FileDownload> OpenFileAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        return await TryOpenFileAsync(ownerId, id, cancellationToken)
            ?? throw new InvalidItemRequestException("A folder has no content.");
    }

    public async Task<FileDownload?> TryOpenFileAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        Item item = (await _items.FindActiveAsync(ownerId, id, cancellationToken)).Item;
        return item.StorageKey is { } key
            ? new FileDownload(item.Name, _mimeTypes.MimeTypeOf(item.Name), item.Modified, _blobs.OpenRead(key))
            : null;
    }

    public async Task<ItemArchive> PrepareArchiveAsync(string ownerId, IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            throw new InvalidItemRequestException("Select at least one item.");
        }

        List<Item> roots = [];
        List<TreeNode> nodes = [];
        foreach (string id in ids)
        {
            Item item = (await _items.FindActiveAsync(ownerId, id, cancellationToken)).Item;
            string name = item.IsRoot ? DefaultArchiveName : item.Name;
            roots.Add(item);
            nodes.AddRange(await _tree.ActiveSubtreeAsync(item, name, cancellationToken));
        }

        return new ItemArchive(ArchiveName(roots), nodes);
    }

    public async Task WriteArchiveAsync(ItemArchive archive, Stream output, CancellationToken cancellationToken)
    {
        await using ZipArchive zip = await ZipArchive.CreateAsync(output, ZipArchiveMode.Create, leaveOpen: true, entryNameEncoding: null, cancellationToken);
        foreach (TreeNode node in archive.Nodes)
        {
            await WriteEntryAsync(zip, node, cancellationToken);
        }
    }

    private static string ArchiveName(List<Item> roots)
    {
        string name = roots is [{ IsFolder: true, IsRoot: false } folder] ? folder.Name : DefaultArchiveName;
        return name + ArchiveExtension;
    }

    private async Task WriteEntryAsync(ZipArchive zip, TreeNode node, CancellationToken cancellationToken)
    {
        string? key = node.Item.StorageKey;
        ZipArchiveEntry entry = key is null
            ? zip.CreateEntry(node.Path + "/")
            : zip.CreateEntry(node.Path, CompressionLevel.Fastest);
        entry.LastWriteTime = node.Item.Modified;
        await using Stream target = await entry.OpenAsync(cancellationToken);
        if (key is not null)
        {
            await using Stream source = _blobs.OpenRead(key);
            await source.CopyToAsync(target, cancellationToken);
        }
    }
}
