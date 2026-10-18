using Guardou.Application.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Guardou.Infrastructure.Identity;

internal sealed class IdentitySessionService(SignInManager<IdentityUser> signInManager) : ISessionService
{
    public async Task<bool> SignInAsync(string username, string password, bool persistent)
    {
        SignInResult result = await signInManager.PasswordSignInAsync(username, password, persistent, lockoutOnFailure: false);
        return result.Succeeded;
    }

    public Task SignOutAsync()
    {
        return signInManager.SignOutAsync();
    }

    public async Task RefreshAsync(string userId)
    {
        if (await signInManager.UserManager.FindByIdAsync(userId) is { } user)
        {
            await signInManager.RefreshSignInAsync(user);
        }
    }
}
