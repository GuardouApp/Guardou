using Guardou.Application.Files;
using Guardou.Infrastructure.Storage;

namespace Guardou.Infrastructure.Files;

internal sealed class LocalBlobStore(StoragePaths paths) : IBlobStore
{
    private const string PartialSuffix = ".part";
    private const int BufferSize = 81920;

    public Task<long> WriteAsync(string key, Stream content, DateTimeOffset modified, CancellationToken cancellationToken)
    {
        return WriteAsync(key, content, modified, replace: false, cancellationToken);
    }

    public async Task CopyAsync(string sourceKey, string targetKey, CancellationToken cancellationToken)
    {
        string source = PathOf(sourceKey);
        await using FileStream content = OpenFile(source);
        await WriteAsync(targetKey, content, File.GetLastWriteTimeUtc(source), replace: true, cancellationToken);
    }

    private async Task<long> WriteAsync(string key, Stream content, DateTimeOffset modified, bool replace, CancellationToken cancellationToken)
    {
        string path = PathOf(key);
        string partial = path + PartialSuffix;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? paths.FilesDirectory);
        try
        {
            await using (FileStream target = CreateFile(partial))
            {
                await content.CopyToAsync(target, BufferSize, cancellationToken);
            }

            File.SetLastWriteTimeUtc(partial, modified.UtcDateTime);
            File.Move(partial, path, replace);
            return new FileInfo(path).Length;
        }
        catch
        {
            File.Delete(partial);
            throw;
        }
    }

    public Stream OpenRead(string key)
    {
        return OpenFile(PathOf(key));
    }

    public bool Exists(string key)
    {
        return File.Exists(PathOf(key));
    }

    public void Delete(string key)
    {
        string path = PathOf(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public long TotalBytes()
    {
        Directory.CreateDirectory(paths.FilesDirectory);
        return new DriveInfo(Path.GetFullPath(paths.FilesDirectory)).TotalSize;
    }

    private static FileStream CreateFile(string path)
    {
        return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous);
    }

    private static FileStream OpenFile(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.Asynchronous);
    }

    public string PathOf(string key)
    {
        return SafePathResolver.Resolve(paths.FilesDirectory, key)
            ?? throw new InvalidOperationException($"The storage key '{key}' leaves the files directory.");
    }
}
