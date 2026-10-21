using Guardou.Domain.Users;

namespace Guardou.UnitTests.Domain;

public sealed class DisplayNameRuleTests
{
    [Theory]
    [InlineData("Alice Smith", true)]
    [InlineData("José Ñ 李", true)]
    [InlineData("", false)]
    [InlineData(" Alice", false)]
    [InlineData("Alice ", false)]
    public void ValidatesTheDisplayName(string displayName, bool expected)
    {
        Assert.Equal(expected, DisplayNameRule.IsValid(displayName));
    }

    [Fact]
    public void RefusesALongDisplayName()
    {
        Assert.True(DisplayNameRule.IsValid(new string('a', DisplayNameRule.MaximumLength)));
        Assert.False(DisplayNameRule.IsValid(new string('a', DisplayNameRule.MaximumLength + 1)));
    }

    [Theory]
    [InlineData("  Alice Smith ", "Alice Smith")]
    [InlineData(null, "alice")]
    [InlineData("   ", "alice")]
    public void SanitizesAnExternalValue(string? value, string expected)
    {
        Assert.Equal(expected, DisplayNameRule.Sanitize(value, "alice"));
    }

    [Fact]
    public void CutsALongExternalValueWithoutSplittingACharacter()
    {
        string value = new string('a', DisplayNameRule.MaximumLength - 1) + "😀";

        string displayName = DisplayNameRule.Sanitize(value, "alice");

        Assert.Equal(new string('a', DisplayNameRule.MaximumLength - 1), displayName);
        Assert.True(DisplayNameRule.IsValid(displayName));
    }
}
