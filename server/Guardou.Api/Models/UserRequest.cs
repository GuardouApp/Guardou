using System.ComponentModel.DataAnnotations;
using Guardou.Domain.Users;

namespace Guardou.Api.Models;

public sealed record UserRequest(
    [Required] string Username,
    [Required, StringLength(DisplayNameRule.MaximumLength)] string DisplayName,
    [Required, EmailAddress] string Email,
    string? Password,
    bool IsAdmin,
    [Range(typeof(long), "0", "9223372036854775807")] long? QuotaBytes = null);
