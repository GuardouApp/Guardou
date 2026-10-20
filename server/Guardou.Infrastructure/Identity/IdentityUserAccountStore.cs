using System.Security.Claims;
using Guardou.Application.Authentication;
using Guardou.Application.Users;
using Guardou.Domain.Files;
using Guardou.Domain.Users;
using Guardou.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Guardou.Infrastructure.Identity;

internal sealed class IdentityUserAccountStore(UserManager<IdentityUser> userManager, AppDbContext context, TimeProvider time)
    : IUserAccountStore
{
    public Task<bool> AnyUserAsync()
    {
        return userManager.Users.AnyAsync();
    }

    public async Task<IReadOnlyList<UserAccount>> ListAsync()
    {
        IList<IdentityUser> administrators = await userManager.GetUsersInRoleAsync(Roles.Admin);
        HashSet<string> administratorIds = administrators.Select(user => user.Id).ToHashSet();
        ILookup<string, string> logins = (await context.UserLogins
                .Select(login => new { login.UserId, login.LoginProvider })
                .ToListAsync())
            .ToLookup(login => login.UserId, login => login.LoginProvider);
        List<IdentityUser> users = await userManager.Users.ToListAsync();
        return users.Select(user => ToAccount(user, administratorIds.Contains(user.Id), logins[user.Id].ToList())).ToList();
    }

    public async Task<UserAccount?> FindAsync(string id)
    {
        return await ToAccountAsync(await userManager.FindByIdAsync(id));
    }

    public async Task<UserAccount?> FindByUsernameAsync(string username)
    {
        return await ToAccountAsync(await userManager.FindByNameAsync(username));
    }

    public async Task<UserAccount?> FindByEmailAsync(string email)
    {
        return await ToAccountAsync(await userManager.FindByEmailAsync(email));
    }

    public async Task<UserAccount?> FindByLoginAsync(string provider, string key)
    {
        return await ToAccountAsync(await userManager.FindByLoginAsync(provider, key));
    }

    public Task<UserAccountResult> CreateAsync(UserProfile profile, string password)
    {
        return CreateAsync(profile, user => userManager.CreateAsync(user, password));
    }

    public Task<UserAccountResult> CreateExternalAsync(UserProfile profile, string provider, string key)
    {
        return CreateAsync(profile, async user =>
        {
            IdentityResult created = await userManager.CreateAsync(user);
            return created.Succeeded ? await userManager.AddLoginAsync(user, LoginOf(provider, key)) : created;
        });
    }

    public async Task<UserAccountResult> UpdateAsync(string id, UserProfile profile, string? password)
    {
        IdentityUser? user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(id);
        }

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        UserAccountResult result = await ApplyAsync(user, profile, password);
        if (result.Succeeded)
        {
            await transaction.CommitAsync();
        }

        return result;
    }

    public async Task<UserAccountResult> AddLoginAsync(string id, string provider, string key)
    {
        IdentityUser? user = await userManager.FindByIdAsync(id);
        return user is null ? NotFound(id) : ToResult(await userManager.AddLoginAsync(user, LoginOf(provider, key)));
    }

    public async Task RemoveLoginAsync(string id, string provider)
    {
        if (await userManager.FindByIdAsync(id) is not { } user)
        {
            return;
        }

        foreach (UserLoginInfo login in (await userManager.GetLoginsAsync(user)).Where(login => login.LoginProvider == provider))
        {
            await userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
        }
    }

    public async Task SetPictureAsync(string id, string? pictureUrl)
    {
        if (await userManager.FindByIdAsync(id) is not { } user)
        {
            return;
        }

        List<Claim> pictures = (await userManager.GetClaimsAsync(user)).Where(claim => claim.Type == OidcClaimTypes.Picture).ToList();
        string[] wanted = pictureUrl is null ? [] : [pictureUrl];
        if (pictures.Select(claim => claim.Value).SequenceEqual(wanted))
        {
            return;
        }

        await userManager.RemoveClaimsAsync(user, pictures);
        if (pictureUrl is not null)
        {
            await userManager.AddClaimAsync(user, new Claim(OidcClaimTypes.Picture, pictureUrl));
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (await userManager.FindByIdAsync(id) is { } user)
        {
            await userManager.DeleteAsync(user);
        }
    }

    private async Task<UserAccountResult> CreateAsync(UserProfile profile, Func<IdentityUser, Task<IdentityResult>> create)
    {
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        IdentityUser user = new(profile.Username) { Email = profile.Email };
        UserAccountResult result = ToResult(await create(user));
        if (result.Succeeded && profile.IsAdmin)
        {
            result = ToResult(await userManager.AddToRoleAsync(user, Roles.Admin));
        }

        if (!result.Succeeded)
        {
            return result;
        }

        context.Set<Item>().Add(Item.CreateRoot(user.Id, time.GetUtcNow()));
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return result;
    }

    private async Task<UserAccountResult> ApplyAsync(IdentityUser user, UserProfile profile, string? password)
    {
        if (password is not null)
        {
            UserAccountResult validation = await ValidatePasswordAsync(user, password);
            if (!validation.Succeeded)
            {
                return validation;
            }

            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, password);
        }

        user.UserName = profile.Username;
        user.Email = profile.Email;
        UserAccountResult result = await SetAdministratorAsync(user, profile.IsAdmin);
        return result.Succeeded ? ToResult(await userManager.UpdateSecurityStampAsync(user)) : result;
    }

    private async Task<UserAccountResult> SetAdministratorAsync(IdentityUser user, bool administrator)
    {
        bool current = await userManager.IsInRoleAsync(user, Roles.Admin);
        if (current == administrator)
        {
            return UserAccountResult.Success;
        }

        return ToResult(administrator
            ? await userManager.AddToRoleAsync(user, Roles.Admin)
            : await userManager.RemoveFromRoleAsync(user, Roles.Admin));
    }

    private async Task<UserAccountResult> ValidatePasswordAsync(IdentityUser user, string password)
    {
        List<UserAccountError> errors = [];
        foreach (IPasswordValidator<IdentityUser> validator in userManager.PasswordValidators)
        {
            IdentityResult result = await validator.ValidateAsync(userManager, user, password);
            errors.AddRange(result.Errors.Select(ToError));
        }

        return new UserAccountResult(errors);
    }

    private async Task<UserAccount?> ToAccountAsync(IdentityUser? user)
    {
        if (user is null)
        {
            return null;
        }

        IList<UserLoginInfo> logins = await userManager.GetLoginsAsync(user);
        return ToAccount(user, await userManager.IsInRoleAsync(user, Roles.Admin), logins.Select(login => login.LoginProvider).ToList());
    }

    private static UserAccount ToAccount(IdentityUser user, bool administrator, IReadOnlyList<string> logins)
    {
        return new UserAccount(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, administrator, logins.Distinct().ToList());
    }

    private static UserLoginInfo LoginOf(string provider, string key)
    {
        return new UserLoginInfo(provider, key, provider);
    }

    private static UserAccountResult NotFound(string id)
    {
        return UserAccountResult.Failure(UserAccountErrorCodes.UserNotFound, $"The user '{id}' does not exist.");
    }

    private static UserAccountResult ToResult(IdentityResult result)
    {
        return new UserAccountResult(result.Errors.Select(ToError).ToList());
    }

    private static UserAccountError ToError(IdentityError error)
    {
        return new UserAccountError(error.Code, error.Description);
    }
}
