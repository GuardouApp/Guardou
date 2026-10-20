using Guardou.Application.Authentication;

namespace Guardou.UnitTests.Authentication;

internal sealed class FakeSessionService : ISessionService
{
    public ExternalLogin? ExternalLogin { get; set; }

    public string? SignedInUserId { get; private set; }

    public Task<bool> SignInAsync(string username, string password, bool persistent)
    {
        SignedInUserId = username;
        return Task.FromResult(true);
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
