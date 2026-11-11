using System.Net;
using Guardou.Application.Files;
using Guardou.Application.Shares;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class ShareRateLimitsTests : SignedInTests
{
    private const string FirstAddress = "198.51.100.1";
    private const string SecondAddress = "198.51.100.2";

    [Theory]
    [InlineData("", 120)]
    [InlineData("/download", 30)]
    [InlineData("/view", 600)]
    public async Task LimitsTheRequestsOfEachAddress(string endpoint, int permits)
    {
        ItemEntry file = await Client.UploadEntryAsync(ItemsApi.Root, "a.txt", "a");
        HttpResponseMessage shared = await Client.PutAsync($"/api/items/{file.Id}/share", null, CancellationToken);
        ShareLink? link = await shared.Content.ReadFromJsonAsync<ShareLink>(HttpClientExtensions.JsonOptions, CancellationToken);
        string path = $"/api/shares/{link?.Url.Split('/')[^1]}/items/{file.Id}{endpoint}";
        using HttpClient anonymous = Factory.CreateClient();

        for (int request = 0; request < permits; request++)
        {
            Assert.Equal(HttpStatusCode.OK, (await GetAsync(anonymous, path, FirstAddress)).StatusCode);
        }

        HttpResponseMessage limited = await GetAsync(anonymous, path, FirstAddress);
        HttpResponseMessage other = await GetAsync(anonymous, path, SecondAddress);

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.NotNull(limited.Headers.RetryAfter);
        Assert.Equal(HttpStatusCode.OK, other.StatusCode);
    }

    private static Task<HttpResponseMessage> GetAsync(HttpClient client, string path, string address)
    {
        HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.Add(RemoteAddressStartupFilter.HeaderName, address);
        return client.SendAsync(request, CancellationToken);
    }

    private protected override TestServerFactory CreateFactory()
    {
        return new TestServerFactory
        {
            TestServices = services => services.AddTransient<IStartupFilter, RemoteAddressStartupFilter>(),
        };
    }
}
