using Guardou.Application.Files;
using Guardou.Domain.Files;

namespace Guardou.Application.Uploads;

public sealed class UploadSessions
{
    private readonly IUploadStore _uploads;
    private readonly IUploadContent _content;
    private readonly OwnedUploads _owned;
    private readonly OwnedItems _items;
    private readonly IItemStore _store;
    private readonly TimeProvider _time;

    internal UploadSessions(
        IUploadStore uploads,
        IUploadContent content,
        OwnedUploads owned,
        OwnedItems items,
        IItemStore store,
        TimeProvider time)
    {
        _uploads = uploads;
        _content = content;
        _owned = owned;
        _items = items;
        _store = store;
        _time = time;
    }

    public async Task<UploadSession?> StartAsync(string ownerId, UploadRequest request, CancellationToken cancellationToken)
    {
        ItemPath target = await _items.FindActiveFolderAsync(ownerId, request.FolderId, cancellationToken);
        UploadDestination destination = UploadDestination.Of(
            target.Item.Id,
            UploadPath.FolderNames(request.RelativePath),
            ItemPlacement.ValidName(request.FileName));
        await EnsureNoActiveUploadAsync(destination, cancellationToken);
        if (await IsTakenAsync(target.Item, destination, cancellationToken))
        {
            switch (request.Conflict)
            {
                case ConflictPolicy.Skip:
                    return null;
                case ConflictPolicy.Fail:
                    throw new ItemConflictException(destination.Name);
            }
        }

        string id = await _content.CreateAsync(request.Size, cancellationToken);
        _uploads.Add(Upload.Create(id, ownerId, destination, request.Size, request.Modified, _time.GetUtcNow()));
        await _uploads.SaveAsync(cancellationToken);
        return new UploadSession(id);
    }

    public async Task CancelAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        Upload upload = await _owned.FindAsync(ownerId, id, cancellationToken);
        await _content.DeleteAsync(upload.Id, cancellationToken);
        _uploads.Remove(upload);
        await _uploads.SaveAsync(cancellationToken);
    }

    public async Task<bool> OwnsAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        Upload? upload = await _uploads.FindAsync(id, cancellationToken);
        return upload?.OwnerId == ownerId;
    }

    public async Task RecordWriteAsync(string id, CancellationToken cancellationToken)
    {
        Upload? upload = await _uploads.FindAsync(id, cancellationToken);
        upload?.RecordWrite(_time.GetUtcNow());
        await _uploads.SaveAsync(cancellationToken);
    }

    private async Task EnsureNoActiveUploadAsync(UploadDestination destination, CancellationToken cancellationToken)
    {
        DateTimeOffset now = _time.GetUtcNow();
        IReadOnlyList<Upload> uploads = await _uploads.ToDestinationAsync(destination, cancellationToken);
        if (uploads.Any(upload => upload.IsActive(now)))
        {
            throw new UploadInProgressException(destination.Name);
        }
    }

    private async Task<bool> IsTakenAsync(Item folder, UploadDestination destination, CancellationToken cancellationToken)
    {
        Item? current = folder;
        foreach (string name in destination.FolderNames)
        {
            current = await _store.FindChildAsync(current.Id, name, cancellationToken);
            if (current is null)
            {
                return false;
            }

            if (!current.IsFolder)
            {
                throw new ItemConflictException(name);
            }
        }

        return await _store.FindChildAsync(current.Id, destination.Name, cancellationToken) is not null;
    }
}
