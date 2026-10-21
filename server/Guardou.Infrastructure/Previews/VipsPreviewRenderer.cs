using Guardou.Application.Previews;
using Guardou.Domain.Files;
using Guardou.Domain.Previews;
using Guardou.Infrastructure.Files;
using NetVips;

namespace Guardou.Infrastructure.Previews;

internal sealed class VipsPreviewRenderer(IEnumerable<IImageSource> sources, LocalBlobStore blobs) : IPreviewRenderer
{
    private const int Quality = 80;
    private const string OrientationField = "orientation";

    private readonly IImageSource[] _sources = sources.ToArray();

    static VipsPreviewRenderer()
    {
        VipsDefaults.Apply();
    }

    public bool Supports(string fileName)
    {
        return SourceFor(fileName) is not null;
    }

    public async Task<IReadOnlyDictionary<PreviewSize, byte[]>> RenderAsync(
        string storageKey,
        string fileName,
        CancellationToken cancellationToken)
    {
        IImageSource source = SourceFor(fileName) ?? throw new InvalidDataException($"The file '{fileName}' has no preview.");
        SourceImage image = await source.ReadAsync(blobs.PathOf(storageKey), cancellationToken);
        return PreviewSizes.All.ToDictionary(size => size, size => Encode(image, size));
    }

    private IImageSource? SourceFor(string fileName)
    {
        string extension = ItemName.Extension(fileName);
        return _sources.FirstOrDefault(source => source.Supports(extension));
    }

    private static byte[] Encode(SourceImage image, PreviewSize size)
    {
        int edge = PreviewSizes.MaximumEdge(size);
        using Image thumbnail = Image.ThumbnailBuffer(image.Content, edge, height: edge, size: Enums.Size.Down);
        using Image oriented = Orient(thumbnail, image.Orientation);
        return oriented.WebpsaveBuffer(q: Quality, keep: Enums.ForeignKeep.None);
    }

    private static Image Orient(Image image, int orientation)
    {
        if (orientation == TiffOrientation.Normal)
        {
            return image.Copy();
        }

        using Image tagged = image.Mutate(mutable => mutable.Set(GValue.GIntType, OrientationField, orientation));
        return tagged.Autorot();
    }
}
