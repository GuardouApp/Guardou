namespace Guardou.Domain.Files;

public sealed record StoredFile(Guid Id, string Name, long Size, string StorageKey, DateTimeOffset Modified);
