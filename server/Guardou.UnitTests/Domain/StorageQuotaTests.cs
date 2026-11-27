using Guardou.Domain.Users;

namespace Guardou.UnitTests.Domain;

public sealed class StorageQuotaTests
{
    [Theory]
    [InlineData(4, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void PermitsUpToTheLimitWithReservations(long bytes, bool expected)
    {
        StorageQuota quota = new(LimitBytes: 10, UsedBytes: 3, ReservedBytes: 2);

        Assert.Equal(expected, quota.Permits(bytes));
    }

    [Fact]
    public void WithoutALimitPermitsAll()
    {
        StorageQuota quota = new(LimitBytes: null, UsedBytes: long.MaxValue / 2, ReservedBytes: 0);

        Assert.True(quota.Permits(long.MaxValue / 2));
    }

    [Theory]
    [InlineData(79, false)]
    [InlineData(80, true)]
    [InlineData(100, true)]
    public void IsNearLimitFromEightyPercent(long used, bool expected)
    {
        StorageQuota quota = new(LimitBytes: 100, UsedBytes: used, ReservedBytes: 0);

        Assert.Equal(expected, quota.IsNearLimit);
    }

    [Fact]
    public void WithoutALimitIsNeverNearLimit()
    {
        Assert.False(new StorageQuota(LimitBytes: null, UsedBytes: long.MaxValue / 200, ReservedBytes: 0).IsNearLimit);
    }

    [Theory]
    [InlineData(70L, 100L, 85L, 100L, true)]
    [InlineData(85L, 100L, 90L, 100L, false)]
    [InlineData(85L, null, 85L, 100L, true)]
    [InlineData(85L, 200L, 85L, 100L, true)]
    [InlineData(85L, 100L, 85L, 200L, false)]
    public void ReachesTheWarningOnlyWhenItCrossesTheThreshold(
        long previousUsed,
        long? previousLimit,
        long currentUsed,
        long? currentLimit,
        bool expected)
    {
        StorageQuota previous = new(previousLimit, previousUsed, ReservedBytes: 0);
        StorageQuota current = new(currentLimit, currentUsed, ReservedBytes: 0);

        Assert.Equal(expected, current.ReachedWarningSince(previous));
    }
}
