namespace Guardou.Application.Users;

public sealed record UserSecurity(string? Password, long? QuotaBytes, bool PasswordChangeRequired);
