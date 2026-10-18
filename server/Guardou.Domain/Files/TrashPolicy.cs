namespace Guardou.Domain.Files;

public static class TrashPolicy
{
    public static TimeSpan Retention { get; } = TimeSpan.FromDays(30);

    public static DateTimeOffset ExpiresAt(DateTimeOffset trashed)
    {
        return trashed + Retention;
    }
}
