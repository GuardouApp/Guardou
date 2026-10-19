using Guardou.Infrastructure.Files;

namespace Guardou.UnitTests.Files;

public sealed class MimeTypeMapTests
{
    private readonly MimeTypeMap _map = new();

    [Theory]
    [InlineData("photo.heic", "image/heic")]
    [InlineData("PHOTO.HEIF", "image/heif")]
    [InlineData("raw.CR2", "image/x-canon-cr2")]
    [InlineData("raw.dng", "image/x-adobe-dng")]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("data.unknown", "application/octet-stream")]
    public void MapsTheExtensionToTheMimeType(string name, string mimeType)
    {
        Assert.Equal(mimeType, _map.MimeTypeOf(name));
    }
}
