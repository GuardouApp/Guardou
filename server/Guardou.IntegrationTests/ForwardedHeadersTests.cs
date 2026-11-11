using System.Net.Http.Json;
using Guardou.Application.Files;
using Guardou.Application.Shares;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class ForwardedHeadersTests : SignedInTests
{
    [Theory]
    [InlineData("10.1.2.3", "https://localhost/s/")]
    [InlineData("192.0.2.1", "http://localhost/s/")]
    [InlineData("203.0.113.7", "https://localhost/s/")]
    public async Task AppliesTheForwardedProtocolOnlyFromTrustedProxies(string proxy, string prefix)
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "a");
        (await Client.PutAsync($"/api/items/{file.Id}/share", null, CancellationToken)).EnsureSuccessStatusCode();
        using HttpRequestMessage request = new(HttpMethod.Get, $"/api/items/{file.Id}/share");
        request.Headers.Add(RemoteAddressStartupFilter.HeaderName, proxy);
        request.Headers.Add("X-Forwarded-Proto", "https");

        HttpResponseMessage response = await Client.SendAsync(request, CancellationToken);
        ShareLink? link = await response.Content.ReadFromJsonAsync<ShareLink>(HttpClientExtensions.JsonOptions, CancellationToken);

        Assert.StartsWith(prefix, link?.Url, StringComparison.Ordinal);
    }

    private protected override TestServerFactory CreateFactory()
    {
        return new TestServerFactory
        {
            Settings = new Dictionary<string, string> { ["Proxy:TrustedProxies"] = "10.0.0.0/8 203.0.113.7" },
            TestServices = services => services.AddTransient<IStartupFilter, RemoteAddressStartupFilter>(),
        };
    }
}
