using Guardou.Domain.Users;

namespace Guardou.UnitTests.Domain;

public sealed class UsernameRuleTests
{
    [Theory]
    [InlineData("admin")]
    [InlineData("Bob42")]
    [InlineData("7")]
    public void AcceptsAlphanumericUsernames(string username)
    {
        Assert.True(UsernameRule.IsValid(username));
    }

    [Theory]
    [InlineData("")]
    [InlineData("john.doe")]
    [InlineData("john doe")]
    [InlineData("john_doe")]
    [InlineData("jöhn")]
    [InlineData("john@example")]
    public void RefusesOtherUsernames(string username)
    {
        Assert.False(UsernameRule.IsValid(username));
    }
}
