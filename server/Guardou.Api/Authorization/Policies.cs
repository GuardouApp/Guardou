using Guardou.Domain.Users;
using Microsoft.AspNetCore.Authorization;

namespace Guardou.Api.Authorization;

internal static class Policies
{
    public const string Admin = "Admin";
    public const string PasswordChange = "PasswordChange";

    public static void Add(AuthorizationBuilder authorization)
    {
        authorization.SetDefaultPolicy(Current().Build());
        authorization.AddPolicy(Admin, Current().RequireRole(Roles.Admin).Build());
        authorization.AddPolicy(PasswordChange, policy => policy.RequireAuthenticatedUser());
    }

    private static AuthorizationPolicyBuilder Current()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PasswordCurrentRequirement());
    }
}
