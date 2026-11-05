using Guardou.Domain.Files;

namespace Guardou.Application.Uploads;

internal sealed class OwnedUploads(IUploadStore store)
{
    public async Task<Upload> FindAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        Upload? upload = await store.FindAsync(id, cancellationToken);
        return upload is not null && upload.OwnerId == ownerId ? upload : throw new UploadNotFoundException(id);
    }
}
