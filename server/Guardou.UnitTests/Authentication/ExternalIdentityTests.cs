using System.Security.Claims;
using Guardou.Application.Authentication;
using Guardou.Domain.Authentication;

namespace Guardou.UnitTests.Authentication;

public sealed class ExternalIdentityTests
{
    [Fact]
    public void ReadsTheConfiguredClaims()
    {
        OidcSettings settings = OidcSettings.Initial with { UserClaim = "upn" };

        ExternalIdentity? identity = ExternalIdentity.From(
            [
                new Claim("sub", "42"),
                new Claim("upn", "alice"),
                new Claim("email", "alice@example.com"),
                new Claim("email_verified", "True"),
                new Claim("picture", "https://id.example.com/alice.png"),
            ],
            settings);

        Assert.NotNull(identity);
        Assert.Equal("42", identity.Key);
        Assert.Equal("alice", identity.Username);
        Assert.True(identity.EmailVerified);
        Assert.Equal("https://id.example.com/alice.png", identity.PictureUrl);
    }

    [Fact]
    public void NeedsASubject()
    {
        Assert.Null(ExternalIdentity.From([new Claim("email", "alice@example.com")], OidcSettings.Initial));
    }

    [Fact]
    public void IgnoresAPictureThatIsNotAnHttpUrl()
    {
        ExternalIdentity? identity = ExternalIdentity.From([new Claim("sub", "42"), new Claim("picture", "javascript:alert(1)")], OidcSettings.Initial);

        Assert.Null(identity?.PictureUrl);
    }

    [Theory]
    [InlineData("alice.smith", "alice@example.com", "alicesmith")]
    [InlineData(null, "bob.jones@example.com", "bobjones")]
    [InlineData("José", null, "Jos")]
    [InlineData("...", null, "user")]
    public void DerivesAValidUsername(string? username, string? email, string expected)
    {
        ExternalIdentity identity = new("42", username, email, EmailVerified: false, PictureUrl: null);

        Assert.Equal(expected, identity.UsernameBase);
    }
}
