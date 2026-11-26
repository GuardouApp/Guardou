using System.ComponentModel.DataAnnotations;

namespace Guardou.Application.Authentication;

public sealed class AuthenticationLimits
{
    public const string Section = "Authentication";

    [Range(1, 1000)]
    public int RateLimit { get; set; } = 10;

    [Range(1, 100)]
    public int LockoutMaxAttempts { get; set; } = 5;

    [Range(1, 1440)]
    public int LockoutMinutes { get; set; } = 15;
}
