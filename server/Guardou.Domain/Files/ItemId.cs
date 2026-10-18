using System.Buffers.Text;

namespace Guardou.Domain.Files;

public static class ItemId
{
    private const int EncodedLength = 22;
    private const int ByteLength = 16;

    public static string Format(Guid id)
    {
        return Base64Url.EncodeToString(id.ToByteArray(bigEndian: true));
    }

    public static bool TryParse(string? value, out Guid id)
    {
        id = Guid.Empty;
        Span<byte> bytes = stackalloc byte[ByteLength];
        if (value is not { Length: EncodedLength }
            || !Base64Url.IsValid(value, out int decodedLength)
            || decodedLength != ByteLength
            || !Base64Url.TryDecodeFromChars(value, bytes, out int written)
            || written != ByteLength)
        {
            return false;
        }

        id = new Guid(bytes, bigEndian: true);
        return true;
    }
}
