using NetVips;

namespace Guardou.Infrastructure.Previews;

internal interface ISourceImage : IDisposable
{
    int Orientation { get; }

    Image Thumbnail(int edge);
}
