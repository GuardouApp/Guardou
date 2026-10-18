namespace Guardou.Application.Files;

public sealed record TrashEntry(ItemEntry Entry, DateTimeOffset Trashed, DateTimeOffset Expires);
