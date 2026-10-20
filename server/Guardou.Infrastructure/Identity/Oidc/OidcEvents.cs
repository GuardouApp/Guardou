using Guardou.Application.Authentication;
using Guardou.Application.Settings;
using Guardou.Infrastructure.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Guardou.Infrastructure.Identity.Oidc;

internal sealed class OidcEvents(ServerSettingsAdministration server) : OpenIdConnectEvents
{
    private const string FailureCode = "failed";

    public override async Task RedirectToIdentityProvider(RedirectContext context)
    {
        string origin = await server.PublicOriginAsync(context.Request.Origin(), context.HttpContext.RequestAborted);
        context.ProtocolMessage.RedirectUri = OidcEndpoints.Callback(origin);
    }

    public override Task RemoteFailure(RemoteFailureContext context)
    {
        context.Response.Redirect(OidcEndpoints.LoginError(FailureCode));
        context.HandleResponse();
        return Task.CompletedTask;
    }
}
