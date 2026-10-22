using Guardou.Domain.Shares;

namespace Guardou.UnitTests.Domain;

public sealed class ShareTokenTests
{
    [Fact]
    public void CreatesDifferentValidTokens()
    {
        string first = ShareToken.Create();
        string second = ShareToken.Create();

        Assert.Equal(ShareToken.Length, first.Length);
        Assert.True(ShareToken.IsValid(first));
        Assert.NotEqual(first, second);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAA+")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAA")]
    public void RefusesInvalidTokens(string? value)
    {
        Assert.False(ShareToken.IsValid(value));
    }
}
