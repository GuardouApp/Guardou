namespace Guardou.Infrastructure.Profiles;

internal static class BoundedStreamReader
{
    private const int BufferSize = 81920;

    public static async Task<byte[]> ReadAsync(Stream content, long maximumBytes, CancellationToken cancellationToken)
    {
        using MemoryStream buffer = new();
        byte[] chunk = new byte[BufferSize];
        int read;
        while ((read = await content.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > maximumBytes)
            {
                throw TooLarge(maximumBytes);
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    public static InvalidDataException TooLarge(long maximumBytes)
    {
        return new InvalidDataException($"The content is larger than {maximumBytes} bytes.");
    }
}
