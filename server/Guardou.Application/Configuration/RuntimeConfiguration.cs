using Guardou.Domain.Users;

namespace Guardou.Application.Configuration;

public sealed record RuntimeConfiguration(
    string ApplicationName,
    IReadOnlyList<AuthenticationMethod> AuthenticationMethods,
    bool RegistrationOpen,
    PasswordPolicy PasswordPolicy,
    int TrashRetentionDays);
