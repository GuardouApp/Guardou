namespace Guardou.UnitTests.Previews;

internal static class SyntheticJpeg
{
    public static byte[] Create(int entropyLength, byte[]? entropy = null)
    {
        byte[] data = entropy ?? [];
        byte[] padding = Enumerable.Repeat((byte)0x11, entropyLength).ToArray();
        return
        [
            0xFF, 0xD8,
            0xFF, 0xE0, 0x00, 0x06, 0x4A, 0x46, 0x49, 0x46,
            0xFF, 0xDA, 0x00, 0x04, 0x01, 0x00,
            .. data,
            .. padding,
            0xFF, 0xD9,
        ];
    }
}
