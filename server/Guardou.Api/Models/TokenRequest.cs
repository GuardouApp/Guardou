using System.ComponentModel.DataAnnotations;

namespace Guardou.Api.Models;

public sealed record TokenRequest([Required] string Username, [Required] string Password);
