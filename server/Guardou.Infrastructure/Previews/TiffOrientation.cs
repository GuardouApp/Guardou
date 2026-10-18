using System.Buffers.Binary;

namespace Guardou.Infrastructure.Previews;

internal static class TiffOrientation
{
    public const int Normal = 1;

    private const int HeaderLength = 8;
    private const int EntryLength = 12;
    private const ushort OrientationTag = 0x0112;
    private const int MaximumOrientation = 8;

    public static int Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderLength || ByteOrder(data) is not { } littleEndian)
        {
            return Normal;
        }

        long directory = ReadUInt32(data[4..], littleEndian);
        if (directory + 2 > data.Length)
        {
            return Normal;
        }

        int offset = (int)directory;
        int count = ReadUInt16(data[offset..], littleEndian);
        for (int index = 0; index < count; index++)
        {
            int entry = offset + 2 + (index * EntryLength);
            if (entry + EntryLength > data.Length)
            {
                break;
            }

            if (ReadUInt16(data[entry..], littleEndian) == OrientationTag)
            {
                int value = ReadUInt16(data[(entry + 8)..], littleEndian);
                return value is >= Normal and <= MaximumOrientation ? value : Normal;
            }
        }

        return Normal;
    }

    private static bool? ByteOrder(ReadOnlySpan<byte> data)
    {
        return (data[0], data[1]) switch
        {
            ((byte)'I', (byte)'I') => true,
            ((byte)'M', (byte)'M') => false,
            _ => null,
        };
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, bool littleEndian)
    {
        return littleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(data) : BinaryPrimitives.ReadUInt16BigEndian(data);
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> data, bool littleEndian)
    {
        return littleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(data) : BinaryPrimitives.ReadUInt32BigEndian(data);
    }
}
