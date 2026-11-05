using Guardou.Application.Files;

namespace Guardou.Api.Models;

public sealed record CompleteUploadRequest(ConflictPolicy Conflict = ConflictPolicy.Fail);
