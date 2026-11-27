using Guardou.Application.Updates;

namespace Guardou.UnitTests.Updates;

public sealed class ReleaseVersionTests
{
    [Theory]
    [InlineData("v0.19.0", "0.18.0", true)]
    [InlineData("0.18.1", "0.18.0", true)]
    [InlineData("v1.0.0", "0.18.0-beta", true)]
    [InlineData("v0.18.0", "0.18.0", false)]
    [InlineData("v0.17.2", "0.18.0", false)]
    [InlineData("v0.19.0", "", false)]
    [InlineData("latest", "0.18.0", false)]
    public void ComparesTheVersionCore(string candidate, string current, bool expected)
    {
        Assert.Equal(expected, ReleaseVersion.IsNewer(candidate, current));
    }

    [Fact]
    public void NormalizeRemovesTheTagPrefix()
    {
        Assert.Equal("0.19.0", ReleaseVersion.Normalize("v0.19.0"));
    }
}
