namespace Guardou.Infrastructure.Storage;

internal sealed class StoragePaths(string filesRoot, string databaseRoot)
{
    public const string DefaultFilesRoot = "/data";
    public const string DefaultDatabaseRoot = "/db";

    public string FilesDirectory { get; } = Path.GetFullPath(filesRoot);

    public string DatabaseDirectory { get; } = Path.GetFullPath(databaseRoot);

    public string DatabaseFile => Path.Combine(DatabaseDirectory, "app.db");
}
