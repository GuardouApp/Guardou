using System.ComponentModel.DataAnnotations;
using Guardou.Application.Files;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Models;

public sealed class UploadForm
{
    [Required]
    [FromForm(Name = "file")]
    public required IFormFile File { get; init; }

    [FromForm(Name = "relativePath")]
    public string? RelativePath { get; init; }

    [FromForm(Name = "modified")]
    public DateTimeOffset? Modified { get; init; }

    [FromForm(Name = "conflict")]
    public ConflictPolicy Conflict { get; init; } = ConflictPolicy.Fail;
}
