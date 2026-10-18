using Guardou.Domain.Previews;

namespace Guardou.Application.Previews;

public interface IPreviewCache
{
    Stream? TryOpen(string ownerId, Guid previewId, PreviewSize size);

    Task WriteAsync(string ownerId, Guid previewId, PreviewSize size, byte[] content, CancellationToken cancellationToken);

    void Delete(string ownerId, Guid previewId);

    void DeleteOwner(string ownerId);
}
