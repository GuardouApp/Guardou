using Guardou.Application.Files;

namespace Guardou.Application.Uploads;

public sealed record UploadRequest(
    string FolderId,
    string FileName,
    string? RelativePath,
    long Size,
    DateTimeOffset? Modified,
    ConflictPolicy Conflict);
