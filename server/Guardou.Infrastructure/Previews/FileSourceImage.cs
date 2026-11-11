using NetVips;

namespace Guardou.Infrastructure.Previews;

internal sealed class FileSourceImage(string path, ScratchDirectory? scratch = null) : ISourceImage
{
    public int Orientation => TiffOrientation.Normal;

    public Image Thumbnail(int edge)
    {
        return Image.Thumbnail(path, edge, height: edge, size: Enums.Size.Down);
    }

    public void Dispose()
    {
        scratch?.Dispose();
    }
}
