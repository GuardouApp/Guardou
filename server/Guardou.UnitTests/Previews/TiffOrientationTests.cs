using Guardou.Infrastructure.Previews;

namespace Guardou.UnitTests.Previews;

public sealed class TiffOrientationTests
{
    [Fact]
    public void ReadsLittleEndianOrientation()
    {
        Assert.Equal(6, TiffOrientation.Read(TiffHeader.LittleEndian(orientation: 6)));
    }

    [Fact]
    public void ReadsBigEndianOrientation()
    {
        byte[] header =
        [
            (byte)'M', (byte)'M', 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08,
            0x00, 0x01,
            0x01, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x08, 0x00, 0x00,
        ];

        Assert.Equal(8, TiffOrientation.Read(header));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public void RefusesInvalidOrientations(int orientation)
    {
        Assert.Equal(TiffOrientation.Normal, TiffOrientation.Read(TiffHeader.LittleEndian(orientation)));
    }

    [Fact]
    public void ReturnsNormalForOtherFormats()
    {
        Assert.Equal(TiffOrientation.Normal, TiffOrientation.Read("FUJIFILMCCD-RAW"u8));
    }

    [Fact]
    public void ReturnsNormalForTruncatedDirectories()
    {
        byte[] header = TiffHeader.LittleEndian(orientation: 6);

        Assert.Equal(TiffOrientation.Normal, TiffOrientation.Read(header.AsSpan(0, 12)));
    }
}
