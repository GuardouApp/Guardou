using Guardou.Application.Previews;
using Guardou.Domain.Previews;
using Guardou.Infrastructure.Files;
using Guardou.Infrastructure.Storage;

namespace Guardou.Infrastructure.Previews;

internal sealed class FilePreviewCache(StoragePaths paths) : IPreviewCache
{
    private const string Extension = ".webp";
    private const string PartialSuffix = ".part";

    public Stream? TryOpen(string ownerId, Guid previewId, PreviewSize size)
    {
        try
        {
            return new FileStream(PathOf(ownerId, previewId, size), FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return null;
        }
    }

    public async Task WriteAsync(string ownerId, Guid previewId, PreviewSize size, byte[] content, CancellationToken cancellationToken)
    {
        string path = PathOf(ownerId, previewId, size);
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

    public void Delete(string ownerId, Guid previewId)
    {
        if (!Directory.Exists(OwnerDirectory(ownerId)))
        {
            return;
        }

        foreach (PreviewSize size in PreviewSizes.All)
        {
            File.Delete(PathOf(ownerId, previewId, size));
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

    private string PathOf(string ownerId, Guid previewId, PreviewSize size)
    {
        string fileName = $"{previewId:N}.{size.ToString().ToLowerInvariant()}{Extension}";
        return Path.Combine(OwnerDirectory(ownerId), fileName);
    }

    private string OwnerDirectory(string ownerId)
    {
        return SafePathResolver.Resolve(paths.PreviewsDirectory, ownerId)
            ?? throw new InvalidOperationException($"The owner '{ownerId}' leaves the preview directory.");
    }
}
