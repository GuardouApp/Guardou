using System.Security.Claims;
using Guardou.Application.Authentication;
using Guardou.Application.Users;
using Guardou.Domain.Authentication;
using Guardou.UnitTests.Settings;
using Guardou.UnitTests.Users;

namespace Guardou.UnitTests.Authentication;

public sealed class ExternalSignInTests
{
    private const string Subject = "subject-1";

    private readonly FakeSettingsStore _settings = new();
    private readonly FakeUserAccountStore _store = new();
    private readonly FakeProfilePictureStore _pictures = new();
    private readonly FakeSessionService _sessions = new();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static OidcSettings Configured => OidcSettings.Initial with { Issuer = "https://id.example.com", ClientId = "guardou" };

    [Fact]
    public async Task FailsWhenOidcIsNotConfigured()
    {
        _sessions.ExternalLogin = LoginOf(("email", "alice@example.com"));

        Assert.Equal(ExternalSignInStatus.Failed, await SignInAsync());
        Assert.Null(_sessions.SignedInUserId);
    }

    [Fact]
    public async Task CreatesAndLinksAUserOnTheFirstLogin()
    {
        await ConfigureAsync(Configured);
        _sessions.ExternalLogin = LoginOf(("preferred_username", "alice.smith"), ("email", "alice@example.com"));

        ExternalSignInStatus status = await SignInAsync();

        UserAccount alice = _store.ByUsername("alicesmith");
        Assert.Equal(ExternalSignInStatus.SignedIn, status);
        Assert.Equal("alice@example.com", alice.Email);
        Assert.Equal([OidcSettings.Provider], alice.ExternalLogins);
        Assert.False(alice.IsAdmin);
        Assert.Equal(alice.Id, _sessions.SignedInUserId);
    }

    [Fact]
    public async Task NumbersAUsernameThatIsTaken()
    {
        await ConfigureAsync(Configured);
        await _store.CreateAsync(new UserProfile("alice", "Alice", "other@example.com", IsAdmin: false), "password");
        _sessions.ExternalLogin = LoginOf(("preferred_username", "alice"), ("email", "alice@example.com"));

        await SignInAsync();

        Assert.Equal("alice@example.com", _store.ByUsername("alice2").Email);
    }

    [Fact]
    public async Task RefusesAnUnknownUserWhenAutoCreationIsDisabled()
    {
        await ConfigureAsync(Configured with { AutoCreateUsers = false });
        _sessions.ExternalLogin = LoginOf(("email", "alice@example.com"));

        Assert.Equal(ExternalSignInStatus.NoAccount, await SignInAsync());
        Assert.Empty(_store.Accounts);
    }

    [Fact]
    public async Task LinksAnExistingUserWithAVerifiedEmail()
    {
        await ConfigureAsync(Configured with { AutoCreateUsers = false });
        await _store.CreateAsync(new UserProfile("alice", "Alice", "alice@example.com", IsAdmin: false), "password");
        _sessions.ExternalLogin = LoginOf(("email", "ALICE@example.com"), ("email_verified", "true"));

        ExternalSignInStatus status = await SignInAsync();

        Assert.Equal(ExternalSignInStatus.SignedIn, status);
        Assert.Equal([OidcSettings.Provider], _store.ByUsername("alice").ExternalLogins);
    }

    [Fact]
    public async Task DoesNotLinkAnUnverifiedEmail()
    {
        await ConfigureAsync(Configured);
        await _store.CreateAsync(new UserProfile("alice", "Alice", "alice@example.com", IsAdmin: false), "password");
        _sessions.ExternalLogin = LoginOf(("email", "alice@example.com"), ("email_verified", "false"));

        Assert.Equal(ExternalSignInStatus.Rejected, await SignInAsync());
        Assert.Empty(_store.ByUsername("alice").ExternalLogins);
    }

    [Fact]
    public async Task CopiesTheDisplayNameAndThePictureOnlyAtCreation()
    {
        await ConfigureAsync(Configured);
        _sessions.ExternalLogin = LoginOf(
            ("email", "alice@example.com"),
            ("display_name", "  Alice Smith "),
            ("picture", "https://id.example.com/alice.png"));
        await SignInAsync();
        string aliceId = _store.ByUsername("alice").Id;

        _sessions.ExternalLogin = LoginOf(
            ("email", "alice@example.com"),
            ("display_name", "Other"),
            ("picture", "https://id.example.com/other.png"));
        await SignInAsync();

        Assert.Equal("Alice Smith", _store.ByUsername("alice").DisplayName);
        Assert.Equal([(aliceId, new Uri("https://id.example.com/alice.png"))], _pictures.Imports);
    }

    [Fact]
    public async Task UsesTheUsernameWhenTheProviderGivesNoDisplayName()
    {
        await ConfigureAsync(Configured);
        _sessions.ExternalLogin = LoginOf(("preferred_username", "alice"), ("email", "alice@example.com"));

        await SignInAsync();

        Assert.Equal("alice", _store.ByUsername("alice").DisplayName);
        Assert.Empty(_pictures.Imports);
    }

    [Fact]
    public async Task KeepsTheTypeOfAnExistingAdmin()
    {
        await ConfigureAsync(Configured);
        await _store.CreateExternalAsync(new UserProfile("root", "Root", "root@example.com", IsAdmin: true), OidcSettings.Provider, Subject);
        _sessions.ExternalLogin = LoginOf(("email", "root@example.com"));

        await SignInAsync();

        Assert.True(_store.ByUsername("root").IsAdmin);
        Assert.Equal(0, _store.Updates);
    }

    private static ExternalLogin LoginOf(params (string Type, string Value)[] claims)
    {
        return new ExternalLogin([new Claim("sub", Subject), .. claims.Select(claim => new Claim(claim.Type, claim.Value))], IdToken: null);
    }

    private Task ConfigureAsync(OidcSettings settings)
    {
        return _settings.SaveAsync(settings, CancellationToken);
    }

    private Task<ExternalSignInStatus> SignInAsync()
    {
        return new ExternalSignIn(TestSettings.Oidc(_settings), _store, _pictures, _sessions).SignInAsync(CancellationToken);
    }
}
