using System.Collections.Frozen;

namespace Guardou.Infrastructure.Previews;

internal sealed class VipsImageSource : IImageSource
{
    private static readonly FrozenSet<string> Extensions = FrozenSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "avif", "gif", "jpe", "jpeg", "jpg", "png", "tif", "tiff", "webp");

    public bool Supports(string extension)
    {
        return Extensions.Contains(extension);
    }

    public Task<ISourceImage> ReadAsync(string path, CancellationToken cancellationToken)
    {
        return Task.FromResult<ISourceImage>(new FileSourceImage(path));
    }
}
