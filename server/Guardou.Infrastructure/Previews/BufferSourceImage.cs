using NetVips;

namespace Guardou.Infrastructure.Previews;

internal sealed class BufferSourceImage(byte[] content, int orientation) : ISourceImage
{
    public int Orientation => orientation;

    public Image Thumbnail(int edge)
    {
        return Image.ThumbnailBuffer(content, edge, height: edge, size: Enums.Size.Down);
    }

    public void Dispose()
    {
    }
}
