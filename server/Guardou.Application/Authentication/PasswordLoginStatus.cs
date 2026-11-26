namespace Guardou.Application.Authentication;

public enum PasswordLoginStatus
{
    SignedIn,
    InvalidCredentials,
    LockedOut,
    Disabled,
}
