using Guardou.Application.Users;
using Guardou.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Guardou.Infrastructure.Identity;

internal sealed class DisplayNameValidator : IUserValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        IdentityResult result = DisplayNameRule.IsValid(user.DisplayName)
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError
            {
                Code = UserAccountErrorCodes.InvalidDisplayName,
                Description = $"The display name must have 1 to {DisplayNameRule.MaximumLength} characters.",
            });
        return Task.FromResult(result);
    }
}
