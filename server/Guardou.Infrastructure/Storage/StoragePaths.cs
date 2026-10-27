namespace Guardou.Infrastructure.Storage;

internal sealed class StoragePaths(string root)
{
    public const string DefaultRoot = "/data";

    public string FilesDirectory { get; } = Path.Combine(Path.GetFullPath(root), "files");

    public string DatabaseDirectory { get; } = Path.Combine(Path.GetFullPath(root), "db");

    public string DatabaseFile => Path.Combine(DatabaseDirectory, "app.db");

    public string IndexDatabaseFile => Path.Combine(DatabaseDirectory, "index.db");

    public string ProfileDirectory { get; } = Path.Combine(Path.GetFullPath(root), "profile");

    public string ThumbsDirectory { get; } = Path.Combine(Path.GetFullPath(root), "thumbs");

    public string TemporaryDirectory { get; } = Path.Combine(Path.GetFullPath(root), "cache", "tmp");
}
