namespace Guardou.Infrastructure.Storage;

internal sealed class StoragePaths(string filesRoot, string databaseRoot, string cacheRoot)
{
    public const string DefaultFilesRoot = "/data";
    public const string DefaultDatabaseRoot = "/db";
    public const string DefaultCacheRoot = "/cache";

    public string FilesDirectory { get; } = Path.GetFullPath(filesRoot);

    public string DatabaseDirectory { get; } = Path.GetFullPath(databaseRoot);

    public string DatabaseFile => Path.Combine(DatabaseDirectory, "app.db");

    public string PreviewsDirectory { get; } = Path.Combine(Path.GetFullPath(cacheRoot), "previews");

    public string TemporaryDirectory { get; } = Path.Combine(Path.GetFullPath(cacheRoot), "tmp");
}
