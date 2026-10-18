namespace Guardou.Application.Files;

public interface IBlobStore
{
    Task<long> WriteAsync(string key, Stream content, DateTimeOffset modified, CancellationToken cancellationToken);

    Task CopyAsync(string sourceKey, string targetKey, CancellationToken cancellationToken);

    Stream OpenRead(string key);

    void Delete(string key);

    long TotalBytes();
}
