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

    public async Task<SourceImage> ReadAsync(string path, CancellationToken cancellationToken)
    {
        return new SourceImage(await SourceFile.ReadAsync(path, cancellationToken), TiffOrientation.Normal);
    }
}
