using Guardou.Api.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Guardou.Api.Authorization;

internal sealed class PasswordChangeResultHandler(ProblemResponses problems) : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        bool passwordChangeRequired = authorizeResult.Forbidden
            && authorizeResult.AuthorizationFailure?.FailureReasons.Any(reason => reason.Handler is PasswordCurrentRequirement) == true;
        return passwordChangeRequired
            ? problems.WriteAsync(context, StatusCodes.Status403Forbidden, "The password must be changed.", PasswordCurrentRequirement.Code)
            : _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
