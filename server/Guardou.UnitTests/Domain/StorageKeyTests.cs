using Guardou.Domain.Files;

namespace Guardou.UnitTests.Domain;

public sealed class StorageKeyTests
{
    private static readonly Guid Id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");
    private static readonly DateTimeOffset Date = new(2025, 3, 9, 23, 30, 0, TimeSpan.FromHours(-3));

    [Fact]
    public void UsesTheUtcDateAndTheExtension()
    {
        Assert.Equal("owner/2025/03/10/0f8fad5bd9cb469fa16570867728950e.jpg", StorageKey.For("owner", Id, "Photo.JPG", Date));
    }

    [Theory]
    [InlineData("README")]
    [InlineData("weird.e x t")]
    [InlineData("long.abcdefghijklmnopq")]
    public void OmitsUnsafeExtensions(string name)
    {
        Assert.EndsWith("/0f8fad5bd9cb469fa16570867728950e", StorageKey.For("owner", Id, name, Date));
    }
}
