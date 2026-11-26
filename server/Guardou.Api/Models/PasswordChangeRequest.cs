using System.ComponentModel.DataAnnotations;

namespace Guardou.Api.Models;

public sealed record PasswordChangeRequest([Required] string CurrentPassword, [Required] string NewPassword);
