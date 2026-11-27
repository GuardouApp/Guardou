namespace Guardou.Api.Realtime;

public sealed record ItemsChangedEvent(IReadOnlyCollection<string> FolderIds);
