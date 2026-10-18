namespace Guardou.Application.Files;

public sealed record UploadRequest(
    string FolderId,
    string FileName,
    string? RelativePath,
    DateTimeOffset? Modified,
    ConflictPolicy Conflict,
    Stream Content);
