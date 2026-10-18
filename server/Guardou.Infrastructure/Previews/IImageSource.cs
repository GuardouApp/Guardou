namespace Guardou.Infrastructure.Previews;

internal interface IImageSource
{
    bool Supports(string extension);

    Task<SourceImage> ReadAsync(string path, CancellationToken cancellationToken);
}
