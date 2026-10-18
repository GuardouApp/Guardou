namespace Guardou.Infrastructure.Previews;

internal sealed class ScratchDirectory : IDisposable
{
    public ScratchDirectory(string root)
    {
        Path = Directory.CreateDirectory(System.IO.Path.Combine(root, Guid.NewGuid().ToString("N"))).FullName;
    }

    public string Path { get; }

    public string Combine(string fileName)
    {
        return System.IO.Path.Combine(Path, fileName);
    }

    public void Dispose()
    {
        Directory.Delete(Path, recursive: true);
    }
}
