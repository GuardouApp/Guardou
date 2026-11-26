using Guardou.Application.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Guardou.Api.Authorization;

internal sealed class PasswordCurrentRequirement : AuthorizationHandler<PasswordCurrentRequirement>, IAuthorizationRequirement
{
    public const string Code = "passwordChangeRequired";

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PasswordCurrentRequirement requirement)
    {
        if (context.User.HasClaim(claim => claim.Type == SessionClaims.PasswordChangeRequired))
        {
            context.Fail(new AuthorizationFailureReason(this, "The password must be changed."));
        }
        else
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
