using Guardou.Application.Previews;
using Guardou.Domain.Previews;
using Guardou.Infrastructure.Files;
using Guardou.Infrastructure.Storage;

namespace Guardou.Infrastructure.Previews;

internal sealed class FilePreviewCache(StoragePaths paths) : IPreviewCache
{
    private const string Extension = ".webp";
    private const string PartialSuffix = ".part";

    public bool Contains(string ownerId, Guid itemId)
    {
        return PreviewSizes.All.All(size => File.Exists(PathOf(ownerId, itemId, size)));
    }

    public Stream? TryOpen(string ownerId, Guid itemId, PreviewSize size)
    {
        try
        {
            return new FileStream(PathOf(ownerId, itemId, size), FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return null;
        }
    }

    public async Task WriteAsync(string ownerId, Guid itemId, PreviewSize size, byte[] content, CancellationToken cancellationToken)
    {
        string path = PathOf(ownerId, itemId, size);
        string partial = path + PartialSuffix;
        Directory.CreateDirectory(OwnerDirectory(ownerId));
        try
        {
            await File.WriteAllBytesAsync(partial, content, cancellationToken);
            File.Move(partial, path, overwrite: true);
        }
        catch
        {
            File.Delete(partial);
            throw;
        }
    }

    public void Delete(string ownerId, Guid itemId)
    {
        if (!Directory.Exists(OwnerDirectory(ownerId)))
        {
            return;
        }

        foreach (PreviewSize size in PreviewSizes.All)
        {
            File.Delete(PathOf(ownerId, itemId, size));
        }
    }

    public void DeleteOwner(string ownerId)
    {
        string directory = OwnerDirectory(ownerId);
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private string PathOf(string ownerId, Guid itemId, PreviewSize size)
    {
        string fileName = $"{itemId:N}.{size.ToString().ToLowerInvariant()}{Extension}";
        return Path.Combine(OwnerDirectory(ownerId), fileName);
    }

    private string OwnerDirectory(string ownerId)
    {
        return SafePathResolver.Resolve(paths.ThumbsDirectory, ownerId)
            ?? throw new InvalidOperationException($"The owner '{ownerId}' leaves the preview directory.");
    }
}
