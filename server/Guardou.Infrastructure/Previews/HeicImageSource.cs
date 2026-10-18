using System.Collections.Frozen;
using Guardou.Infrastructure.Storage;

namespace Guardou.Infrastructure.Previews;

internal sealed class HeicImageSource(StoragePaths paths) : IImageSource
{
    private const string OutputName = "image.jpg";
    private const string Quality = "90";

    private static readonly FrozenSet<string> Extensions = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, "heic", "heif");

    private readonly ExternalTool _decoder = new("heif-dec");

    public bool Supports(string extension)
    {
        return Extensions.Contains(extension) && _decoder.IsAvailable;
    }

    public async Task<SourceImage> ReadAsync(string path, CancellationToken cancellationToken)
    {
        using ScratchDirectory scratch = new(paths.TemporaryDirectory);
        string output = scratch.Combine(OutputName);
        await _decoder.RunAsync(["--quality", Quality, path, output], cancellationToken);
        return new SourceImage(await File.ReadAllBytesAsync(output, cancellationToken), TiffOrientation.Normal);
    }
}
