namespace Guardou.Application.Users;

public static class UserAccountErrorCodes
{
    public const string UserNotFound = nameof(UserNotFound);
    public const string EmailRequired = nameof(EmailRequired);
    public const string PasswordRequired = nameof(PasswordRequired);
    public const string LastAdministrator = nameof(LastAdministrator);
    public const string SelfDeletion = nameof(SelfDeletion);
    public const string InvalidDisplayName = nameof(InvalidDisplayName);
}
