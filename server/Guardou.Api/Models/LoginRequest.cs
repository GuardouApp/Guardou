using System.ComponentModel.DataAnnotations;

namespace Guardou.Api.Models;

public sealed record LoginRequest(
    [Required] string Username,
    [Required] string Password,
    bool RememberMe);
