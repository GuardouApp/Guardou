namespace Guardou.Domain.Users;

public static class UsernameRule
{
    public static bool IsValid(string username)
    {
        return username.Length > 0 && username.All(char.IsAsciiLetterOrDigit);
    }

    public static string Sanitize(string value)
    {
        return string.Concat(value.Where(char.IsAsciiLetterOrDigit));
    }
}
