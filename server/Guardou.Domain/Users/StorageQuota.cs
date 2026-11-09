namespace Guardou.Domain.Users;

public sealed record StorageQuota(long? LimitBytes, long UsedBytes, long ReservedBytes)
{
    public bool Permits(long bytes)
    {
        return LimitBytes is not { } limit || UsedBytes + ReservedBytes + bytes <= limit;
    }
}
