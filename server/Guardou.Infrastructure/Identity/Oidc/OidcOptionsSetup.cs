using Guardou.Application.Authentication;
using Guardou.Domain.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Guardou.Infrastructure.Identity.Oidc;

internal sealed class OidcOptionsSetup(OidcSettingsSnapshot snapshot) : IConfigureNamedOptions<OpenIdConnectOptions>
{
    private const string UnconfiguredClientId = "unconfigured";

    public void Configure(OpenIdConnectOptions options)
    {
        Configure(Options.DefaultName, options);
    }

    public void Configure(string? name, OpenIdConnectOptions options)
    {
        if (name != OidcSettings.Provider)
        {
            return;
        }

        ConfigureProtocol(options);
        if (snapshot.Current is { Issuer: { } issuer, ClientId: { } clientId } settings)
        {
            ConfigureClient(options, settings, issuer, clientId);
        }
        else
        {
            options.ClientId = UnconfiguredClientId;
            options.Configuration = new OpenIdConnectConfiguration();
        }
    }

    private static void ConfigureProtocol(OpenIdConnectOptions options)
    {
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.CallbackPath = OidcEndpoints.CallbackPath;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.ResponseMode = OpenIdConnectResponseMode.Query;
        options.UsePkce = true;
        options.SaveTokens = true;
        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.EventsType = typeof(OidcEvents);
        UseLaxCookie(options.CorrelationCookie);
        UseLaxCookie(options.NonceCookie);
    }

    private static void ConfigureClient(OpenIdConnectOptions options, OidcSettings settings, string issuer, string clientId)
    {
        options.Authority = issuer;
        options.RequireHttpsMetadata = new Uri(issuer).Scheme == Uri.UriSchemeHttps;
        options.ClientId = clientId;
        options.ClientSecret = settings.ClientSecret;
        options.Scope.Clear();
        foreach (string scope in settings.Scopes)
        {
            options.Scope.Add(scope);
        }

        foreach (string claim in new[] { settings.UserClaim, OidcClaimTypes.DisplayName, OidcClaimTypes.EmailVerified, OidcClaimTypes.Picture })
        {
            options.ClaimActions.MapUniqueJsonKey(claim, claim);
        }
    }

    private static void UseLaxCookie(CookieBuilder cookie)
    {
        cookie.SameSite = SameSiteMode.Lax;
        cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    }
}
