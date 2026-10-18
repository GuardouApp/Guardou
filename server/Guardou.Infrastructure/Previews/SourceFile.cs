namespace Guardou.Infrastructure.Previews;

internal static class SourceFile
{
    private const long MaximumBytes = 512L * 1024 * 1024;

    public static Task<byte[]> ReadAsync(string path, CancellationToken cancellationToken)
    {
        return new FileInfo(path).Length > MaximumBytes
            ? throw new InvalidDataException("The file is too large for a preview.")
            : File.ReadAllBytesAsync(path, cancellationToken);
    }
}
