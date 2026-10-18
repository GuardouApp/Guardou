using Guardou.Domain.Files;

namespace Guardou.UnitTests.Domain;

public sealed class ItemIdTests
{
    [Fact]
    public void FormatsAndParsesIds()
    {
        Guid id = Guid.NewGuid();

        string value = ItemId.Format(id);

        Assert.Equal(22, value.Length);
        Assert.True(ItemId.TryParse(value, out Guid parsed));
        Assert.Equal(id, parsed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("root")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAA+")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAA")]
    public void RefusesInvalidIds(string? value)
    {
        Assert.False(ItemId.TryParse(value, out _));
    }
}
