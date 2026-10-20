using Guardou.Application.Authentication;
using Guardou.Application.Settings;
using Guardou.Domain.Authentication;
using Guardou.Infrastructure.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Guardou.Infrastructure.Identity.Oidc;

internal sealed partial class OidcSignOut(
    IOptionsMonitor<OpenIdConnectOptions> monitor,
    ServerSettingsAdministration server,
    ILogger<OidcSignOut> logger)
{
    public async Task<string?> EndSessionUrlAsync(HttpRequest request, string idToken, CancellationToken cancellationToken)
    {
        OpenIdConnectOptions options = monitor.Get(OidcSettings.Provider);
        OpenIdConnectConfiguration? configuration = await ConfigurationAsync(options, cancellationToken);
        if (configuration?.EndSessionEndpoint is not { Length: > 0 } endpoint)
        {
            return null;
        }

        string origin = await server.PublicOriginAsync(request.Origin(), cancellationToken);
        return QueryHelpers.AddQueryString(endpoint, new Dictionary<string, string?>
        {
            [OpenIdConnectParameterNames.IdTokenHint] = idToken,
            [OpenIdConnectParameterNames.PostLogoutRedirectUri] = OidcEndpoints.SignedOut(origin),
            [OpenIdConnectParameterNames.ClientId] = options.ClientId,
        });
    }

    private async Task<OpenIdConnectConfiguration?> ConfigurationAsync(
        OpenIdConnectOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            return options.ConfigurationManager is null
                ? null
                : await options.ConfigurationManager.GetConfigurationAsync(cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            LogMetadataFailed(exception);
            return null;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "The OIDC provider metadata could not be loaded for the sign-out.")]
    private partial void LogMetadataFailed(Exception exception);
}
