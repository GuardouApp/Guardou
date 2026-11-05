using System.ComponentModel.DataAnnotations;
using Guardou.Application.Files;

namespace Guardou.Api.Models;

public sealed record StartUploadRequest(
    [Required] string FolderId,
    [Required] string FileName,
    string? RelativePath,
    [Range(typeof(long), "0", "9223372036854775807")] long Size,
    DateTimeOffset? Modified,
    ConflictPolicy Conflict = ConflictPolicy.Fail);
