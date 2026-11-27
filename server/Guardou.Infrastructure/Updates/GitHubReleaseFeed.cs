using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Guardou.Application.Updates;
using Guardou.Domain;

namespace Guardou.Infrastructure.Updates;

internal sealed class GitHubReleaseFeed(HttpClient client) : IReleaseFeed
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    public const string MediaType = "application/vnd.github+json";

    public static readonly Uri BaseAddress = new("https://api.github.com/");

    private const string LatestRelease = $"repos/{Product.Repository}/releases/latest";

    public async Task<string?> LatestVersionAsync(CancellationToken cancellationToken)
    {
        LatestReleaseResponse? release = await client.GetFromJsonAsync<LatestReleaseResponse>(LatestRelease, cancellationToken);
        return release?.TagName;
    }

    private sealed record LatestReleaseResponse([property: JsonPropertyName("tag_name")] string? TagName);
}
