using Guardou.Domain.Previews;

namespace Guardou.Application.Previews;

public interface IPreviewRenderer
{
    bool Supports(string fileName);

    Task<IReadOnlyDictionary<PreviewSize, byte[]>> RenderAsync(string storageKey, string fileName, CancellationToken cancellationToken);
}
