using System.ComponentModel.DataAnnotations;
using Guardou.Domain.Users;

namespace Guardou.Api.Models;

public sealed record RegisterRequest(
    [Required] string Username,
    [Required, StringLength(DisplayNameRule.MaximumLength)] string DisplayName,
    [Required, EmailAddress] string Email,
    [Required] string Password);
