namespace Guardou.Infrastructure.Storage;

internal sealed class DataDirectory(string root)
{
    public const string DefaultRoot = "/data";

    public string DatabaseDirectory => Path.Combine(Root, "database");

    public string DatabaseFile => Path.Combine(DatabaseDirectory, "app.db");

    public string KeysDirectory => Path.Combine(Root, "keys");

    public string FilesDirectory => Path.Combine(Root, "files");

    private string Root { get; } = Path.GetFullPath(root);
}
