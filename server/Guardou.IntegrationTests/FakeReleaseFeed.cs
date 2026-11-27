using Guardou.Application.Updates;

namespace Guardou.IntegrationTests;

internal sealed class FakeReleaseFeed : IReleaseFeed
{
    public string? Latest { get; set; }

    public Task<string?> LatestVersionAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Latest);
    }
}
