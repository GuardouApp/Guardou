using Guardou.Domain.Files;

namespace Guardou.Application.Uploads;

public sealed class UploadCleanup
{
    private readonly IUploadStore _uploads;
    private readonly IUploadContent _content;
    private readonly TimeProvider _time;

    internal UploadCleanup(IUploadStore uploads, IUploadContent content, TimeProvider time)
    {
        _uploads = uploads;
        _content = content;
        _time = time;
    }

    public async Task RemoveExpiredAsync(CancellationToken cancellationToken)
    {
        await _content.RemoveExpiredAsync(cancellationToken);
        IReadOnlyList<Upload> expired = await _uploads.LastWrittenBeforeAsync(_time.GetUtcNow() - Upload.Lifetime, cancellationToken);
        foreach (Upload upload in expired)
        {
            await _content.DeleteAsync(upload.Id, cancellationToken);
            _uploads.Remove(upload);
        }

        await _uploads.SaveAsync(cancellationToken);
    }
}
