using Guardou.Domain.Previews;

namespace Guardou.Application.Previews;

public interface IPreviewCache
{
    bool Contains(string ownerId, Guid itemId);

    Stream? TryOpen(string ownerId, Guid itemId, PreviewSize size);

    Task WriteAsync(string ownerId, Guid itemId, PreviewSize size, byte[] content, CancellationToken cancellationToken);

    void Delete(string ownerId, Guid itemId);

    void DeleteOwner(string ownerId);
}
