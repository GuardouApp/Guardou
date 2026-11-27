namespace Guardou.Domain.Users;

public sealed record StorageQuota(long? LimitBytes, long UsedBytes, long ReservedBytes)
{
    private const int WarningPercent = 80;

    public bool IsNearLimit => LimitBytes is { } limit && UsedBytes * 100 >= limit * WarningPercent;

    public bool Permits(long bytes)
    {
        return LimitBytes is not { } limit || UsedBytes + ReservedBytes + bytes <= limit;
    }

    public bool ReachedWarningSince(StorageQuota previous)
    {
        return IsNearLimit && !previous.IsNearLimit;
    }
}
