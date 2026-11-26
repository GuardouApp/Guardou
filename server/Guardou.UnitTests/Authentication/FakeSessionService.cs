using Guardou.Application.Authentication;

namespace Guardou.UnitTests.Authentication;

internal sealed class FakeSessionService : ISessionService
{
    public ExternalLogin? ExternalLogin { get; set; }

    public string? SignedInUserId { get; private set; }

    public Task<PasswordLoginStatus> SignInAsync(string username, string password, SessionType type, bool persistent)
    {
        SignedInUserId = username;
        return Task.FromResult(PasswordLoginStatus.SignedIn);
    }

    public Task<bool> RefreshTokenAsync(string refreshToken)
    {
        return Task.FromResult(false);
    }

    public Task<ExternalLogin?> ReadExternalLoginAsync()
    {
        return Task.FromResult(ExternalLogin);
    }

    public Task SignInExternalAsync(string userId, ExternalLogin login)
    {
        SignedInUserId = userId;
        return Task.CompletedTask;
    }

    public Task<string?> SignOutAsync(CancellationToken cancellationToken)
    {
        SignedInUserId = null;
        return Task.FromResult<string?>(null);
    }

    public Task RefreshAsync(string userId)
    {
        return Task.CompletedTask;
    }
}
