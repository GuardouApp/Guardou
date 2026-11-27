namespace Guardou.Application.Updates;

public interface IReleaseFeed
{
    Task<string?> LatestVersionAsync(CancellationToken cancellationToken);
}
