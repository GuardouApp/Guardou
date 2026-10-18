namespace Guardou.Application.Files;

public sealed record FileDownload(string Name, string MimeType, DateTimeOffset Modified, Stream Content);
