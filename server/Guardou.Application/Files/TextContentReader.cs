using System.Text;

namespace Guardou.Application.Files;

internal static class TextContentReader
{
    public const int SizeLimit = 1024 * 1024;

    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static async Task<string?> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[SizeLimit + 1];
        int length = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken);
        if (length > SizeLimit || buffer.AsSpan(0, length).Contains((byte)0))
        {
            return null;
        }

        try
        {
            return StrictUtf8.GetString(buffer, 0, length);
        }
        catch (DecoderFallbackException)
        {
            return null;
        }
    }
}
