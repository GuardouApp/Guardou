namespace Guardou.UnitTests.Files;

internal sealed class TemporaryDirectory : IDisposable
{
    public string Path { get; } = Directory.CreateDirectory(
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "guardou-unit", Guid.NewGuid().ToString("N"))).FullName;

    public string Combine(params string[] segments)
    {
        return System.IO.Path.Combine([Path, .. segments]);
    }

    public void Dispose()
    {
        Directory.Delete(Path, recursive: true);
    }
}
