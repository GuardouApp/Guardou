using System.ComponentModel.DataAnnotations;

namespace Guardou.Api.Models;

public sealed record RefreshRequest([Required] string RefreshToken);
