using Guardou.Application.Files;
using Guardou.Infrastructure.Storage;

namespace Guardou.Infrastructure.Files;

internal sealed class LocalBlobStore(DataDirectory dataDirectory) : IBlobStore
{
    private const string PartialSuffix = ".part";
    private const int BufferSize = 81920;

    public async Task<long> WriteAsync(string key, Stream content, DateTimeOffset modified, CancellationToken cancellationToken)
    {
        string path = PathOf(key);
        string partial = path + PartialSuffix;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? dataDirectory.FilesDirectory);
        try
        {
            await using (FileStream target = CreateFile(partial))
            {
                await content.CopyToAsync(target, BufferSize, cancellationToken);
            }

            File.SetLastWriteTimeUtc(partial, modified.UtcDateTime);
            File.Move(partial, path);
            return new FileInfo(path).Length;
        }
        catch
        {
            File.Delete(partial);
            throw;
        }
    }

    public async Task CopyAsync(string sourceKey, string targetKey, CancellationToken cancellationToken)
    {
        string source = PathOf(sourceKey);
        await using FileStream content = OpenFile(source);
        await WriteAsync(targetKey, content, File.GetLastWriteTimeUtc(source), cancellationToken);
    }

    public Stream OpenRead(string key)
    {
        return OpenFile(PathOf(key));
    }

    public void Delete(string key)
    {
        File.Delete(PathOf(key));
    }

    public long TotalBytes()
    {
        Directory.CreateDirectory(dataDirectory.FilesDirectory);
        return new DriveInfo(Path.GetFullPath(dataDirectory.FilesDirectory)).TotalSize;
    }

    private static FileStream CreateFile(string path)
    {
        return new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous);
    }

    private static FileStream OpenFile(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.Asynchronous);
    }

    private string PathOf(string key)
    {
        return SafePathResolver.Resolve(Path.GetFullPath(dataDirectory.FilesDirectory), key)
            ?? throw new InvalidOperationException($"The storage key '{key}' leaves the files directory.");
    }
}
