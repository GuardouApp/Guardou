using System.ComponentModel.DataAnnotations;

namespace Guardou.Api.Models;

public sealed record RegisterRequest(
    [Required] string Username,
    [Required, EmailAddress] string Email,
    [Required] string Password);
