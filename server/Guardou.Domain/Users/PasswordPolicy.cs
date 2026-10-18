namespace Guardou.Domain.Users;

public sealed record PasswordPolicy(
    int MinimumLength,
    bool RequireDigit,
    bool RequireLowercase,
    bool RequireUppercase,
    bool RequireNonAlphanumeric)
{
    public static PasswordPolicy Default { get; } = new(
        MinimumLength: 8,
        RequireDigit: true,
        RequireLowercase: true,
        RequireUppercase: true,
        RequireNonAlphanumeric: true);
}
