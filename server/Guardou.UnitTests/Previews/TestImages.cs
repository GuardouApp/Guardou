using NetVips;

namespace Guardou.UnitTests.Previews;

internal static class TestImages
{
    public static byte[] Encode(int width, int height, string format, int? orientation = null)
    {
        using Image black = Image.Black(width, height, bands: 3);
        using Image image = orientation is { } value
            ? black.Mutate(mutable => mutable.Set(GValue.GIntType, "orientation", value))
            : black.Copy();
        return image.WriteToBuffer(format);
    }
}
