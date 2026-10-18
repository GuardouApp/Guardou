using System.Buffers.Binary;

namespace Guardou.Infrastructure.Previews;

internal static class EmbeddedJpeg
{
    private const byte MarkerPrefix = 0xFF;
    private const byte StartOfImage = 0xD8;
    private const byte EndOfImage = 0xD9;
    private const byte StartOfScan = 0xDA;
    private const byte FirstRestart = 0xD0;
    private const byte LastRestart = 0xD7;
    private const byte Temporary = 0x01;
    private const byte StuffedZero = 0x00;

    public static ReadOnlyMemory<byte>? FindLargest(ReadOnlyMemory<byte> data)
    {
        ReadOnlySpan<byte> span = data.Span;
        ReadOnlyMemory<byte>? largest = null;
        int start = 0;
        while (NextStart(span, start) is var candidate and >= 0)
        {
            int length = LengthAt(span[candidate..]);
            if (length > 0 && length > (largest?.Length ?? 0))
            {
                largest = data.Slice(candidate, length);
            }

            start = candidate + Math.Max(length, 1);
        }

        return largest;
    }

    private static int NextStart(ReadOnlySpan<byte> span, int from)
    {
        ReadOnlySpan<byte> signature = [MarkerPrefix, StartOfImage, MarkerPrefix];
        int index = span[from..].IndexOf(signature);
        return index < 0 ? -1 : from + index;
    }

    private static int LengthAt(ReadOnlySpan<byte> jpeg)
    {
        int position = 2;
        while (position + 1 < jpeg.Length)
        {
            if (jpeg[position] != MarkerPrefix)
            {
                return -1;
            }

            byte marker = jpeg[position + 1];
            if (marker == MarkerPrefix)
            {
                position++;
            }
            else if (marker == EndOfImage)
            {
                return position + 2;
            }
            else if (IsStandalone(marker))
            {
                position += 2;
            }
            else if (position + 3 < jpeg.Length && BinaryPrimitives.ReadUInt16BigEndian(jpeg[(position + 2)..]) is var segment and >= 2)
            {
                position += 2 + segment;
                if (marker == StartOfScan)
                {
                    position = SkipEntropyData(jpeg, position);
                }
            }
            else
            {
                return -1;
            }
        }

        return -1;
    }

    private static int SkipEntropyData(ReadOnlySpan<byte> jpeg, int position)
    {
        while (position + 1 < jpeg.Length)
        {
            if (jpeg[position] == MarkerPrefix && !IsEntropyEscape(jpeg[position + 1]))
            {
                return position;
            }

            position++;
        }

        return jpeg.Length;
    }

    private static bool IsEntropyEscape(byte marker)
    {
        return marker is StuffedZero or MarkerPrefix || IsRestart(marker);
    }

    private static bool IsStandalone(byte marker)
    {
        return marker == Temporary || IsRestart(marker);
    }

    private static bool IsRestart(byte marker)
    {
        return marker is >= FirstRestart and <= LastRestart;
    }
}
