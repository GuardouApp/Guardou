using Guardou.Infrastructure.Previews;

namespace Guardou.UnitTests.Previews;

public sealed class EmbeddedJpegTests
{
    [Fact]
    public void FindsTheLargestJpeg()
    {
        byte[] small = SyntheticJpeg.Create(entropyLength: 4);
        byte[] large = SyntheticJpeg.Create(entropyLength: 64);
        byte[] raw = [.. "II*\0"u8, 0x10, 0x20, .. small, 0x00, 0x33, .. large, 0x44];

        ReadOnlyMemory<byte>? found = EmbeddedJpeg.FindLargest(raw);

        Assert.Equal(large, found?.ToArray());
    }

    [Fact]
    public void SkipsEscapedBytesInTheEntropyData()
    {
        byte[] jpeg = SyntheticJpeg.Create(entropyLength: 8, entropy: [0xFF, 0x00, 0xFF, 0xD3]);

        ReadOnlyMemory<byte>? found = EmbeddedJpeg.FindLargest(jpeg);

        Assert.Equal(jpeg, found?.ToArray());
    }

    [Fact]
    public void IgnoresTruncatedJpegs()
    {
        byte[] jpeg = SyntheticJpeg.Create(entropyLength: 8);

        ReadOnlyMemory<byte>? found = EmbeddedJpeg.FindLargest(jpeg.AsMemory(0, jpeg.Length - 2));

        Assert.Null(found);
    }
}
