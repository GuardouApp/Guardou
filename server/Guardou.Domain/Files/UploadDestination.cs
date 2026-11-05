namespace Guardou.Domain.Files;

public sealed record UploadDestination(Guid FolderId, string FolderPath, string Name)
{
    private const char Separator = '/';

    public IReadOnlyList<string> FolderNames => FolderPath.Length == 0 ? [] : FolderPath.Split(Separator);

    public static UploadDestination Of(Guid folderId, IEnumerable<string> folderNames, string name)
    {
        return new UploadDestination(folderId, string.Join(Separator, folderNames), name);
    }
}
