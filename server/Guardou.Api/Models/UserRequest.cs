using System.ComponentModel.DataAnnotations;

namespace Guardou.Api.Models;

public sealed record UserRequest(
    [Required] string Username,
    [Required, EmailAddress] string Email,
    string? Password,
    bool IsAdmin);
