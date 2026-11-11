using Guardou.Api.Proxies;

namespace Guardou.IntegrationTests;

public sealed class TrustedNetworksTests
{
    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("10.0.0.0/8", 1)]
    [InlineData("172.18.0.1 fd00::/8", 2)]
    public void ParsesAddressesAndNetworks(string? value, int count)
    {
        Assert.Equal(count, TrustedNetworks.TryParse(value)?.Count);
    }

    [Theory]
    [InlineData("proxy")]
    [InlineData("10.0.0.0/33")]
    [InlineData("10.0.0.1,10.0.0.2")]
    public void RefusesInvalidValues(string value)
    {
        Assert.Null(TrustedNetworks.TryParse(value));
    }
}
