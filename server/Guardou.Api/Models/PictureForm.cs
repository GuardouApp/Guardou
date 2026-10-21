using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Models;

public sealed class PictureForm
{
    [Required]
    [FromForm(Name = "file")]
    public required IFormFile File { get; init; }
}
