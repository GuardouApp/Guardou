using Guardou.Application.Users;

namespace Guardou.Infrastructure.Profiles;

internal sealed class ProfilePictureDownloader(HttpClient client)
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    public async Task<byte[]> DownloadAsync(Uri source, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await client.GetAsync(source, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > ProfilePictureLimits.MaximumBytes)
        {
            throw BoundedStreamReader.TooLarge(ProfilePictureLimits.MaximumBytes);
        }

        await using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await BoundedStreamReader.ReadAsync(content, ProfilePictureLimits.MaximumBytes, cancellationToken);
    }
}
