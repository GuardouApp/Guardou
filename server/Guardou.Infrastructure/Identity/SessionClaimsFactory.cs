using System.Security.Claims;
using Guardou.Application.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Guardou.Infrastructure.Identity;

internal sealed class SessionClaimsFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        if (user.PasswordChangeRequired)
        {
            identity.AddClaim(new Claim(SessionClaims.PasswordChangeRequired, bool.TrueString));
        }

        return identity;
    }
}
