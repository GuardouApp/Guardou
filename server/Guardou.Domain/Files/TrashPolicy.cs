namespace Guardou.Domain.Files;

public sealed record TrashPolicy(int RetentionDays)
{
    public const int DefaultRetentionDays = 30;
    public const int MinimumRetentionDays = 1;
    public const int MaximumRetentionDays = 3650;

    public TimeSpan Retention => TimeSpan.FromDays(RetentionDays);

    public static bool IsValid(int retentionDays)
    {
        return retentionDays is >= MinimumRetentionDays and <= MaximumRetentionDays;
    }

    public DateTimeOffset ExpiresAt(DateTimeOffset trashed)
    {
        return trashed + Retention;
    }

    public DateTimeOffset Cutoff(DateTimeOffset now)
    {
        return now - Retention;
    }
}
