namespace Guardou.Infrastructure.Previews;

internal interface IImageSource
{
    bool Supports(string extension);

    Task<ISourceImage> ReadAsync(string path, CancellationToken cancellationToken);
}
