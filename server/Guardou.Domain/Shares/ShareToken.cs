using System.Buffers.Text;
using System.Security.Cryptography;

namespace Guardou.Domain.Shares;

public static class ShareToken
{
    public const int Length = 22;

    private const int ByteLength = 16;

    public static string Create()
    {
        return Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(ByteLength));
    }

    public static bool IsValid(string? value)
    {
        return value is { Length: Length } && Base64Url.IsValid(value, out int decodedLength) && decodedLength == ByteLength;
    }
}
