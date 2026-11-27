namespace Guardou.Application.Files;

public sealed record ItemChanges(IReadOnlyCollection<ChangedFolder> Folders, IReadOnlyDictionary<string, long> UsageChanges)
{
    public static ItemChanges Of(string ownerId, Guid? folderId)
    {
        return new ItemChanges(folderId is { } id ? [new ChangedFolder(ownerId, id)] : [], new Dictionary<string, long>());
    }
}
