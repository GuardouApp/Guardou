namespace Guardou.Infrastructure.Profiles;

internal sealed class ProfilePictureDownloader(HttpClient client)
{
    public const long MaximumBytes = 5 * 1024 * 1024;
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private const int BufferSize = 81920;

    public async Task<byte[]> DownloadAsync(Uri source, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await client.GetAsync(source, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > MaximumBytes)
        {
            throw TooLarge(source);
        }

        await using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using MemoryStream buffer = new();
        byte[] chunk = new byte[BufferSize];
        int read;
        while ((read = await content.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaximumBytes)
            {
                throw TooLarge(source);
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    private static InvalidDataException TooLarge(Uri source)
    {
        return new InvalidDataException($"The picture '{source}' is larger than {MaximumBytes} bytes.");
    }
}
