using Guardou.Domain.Users;

namespace Guardou.Application.Configuration;

public sealed record RuntimeConfiguration(
    string ApplicationName,
    IReadOnlyList<AuthenticationMethod> AuthenticationMethods,
    bool RegistrationOpen,
    bool SignupOpen,
    PasswordPolicy PasswordPolicy,
    int TrashRetentionDays,
    int UploadConcurrency,
    int UploadChunkSizeMb);
