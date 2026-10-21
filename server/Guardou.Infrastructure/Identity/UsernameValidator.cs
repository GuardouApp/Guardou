using Guardou.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Guardou.Infrastructure.Identity;

internal sealed class UsernameValidator : IUserValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        string username = user.UserName ?? string.Empty;
        IdentityResult result = UsernameRule.IsValid(username)
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError
            {
                Code = nameof(IdentityErrorDescriber.InvalidUserName),
                Description = $"Username '{username}' is invalid. Use only letters and digits.",
            });
        return Task.FromResult(result);
    }
}
