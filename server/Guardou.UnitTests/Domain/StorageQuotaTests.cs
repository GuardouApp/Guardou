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
}
