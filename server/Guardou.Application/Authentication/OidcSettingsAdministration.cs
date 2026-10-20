using Guardou.Application.Settings;
using Guardou.Domain.Authentication;

namespace Guardou.Application.Authentication;

public sealed class OidcSettingsAdministration(
    ISettingsStore store,
    ServerSettingsAdministration server,
    IOidcSettingsObserver observer)
{
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        observer.Changed(await store.GetAsync<OidcSettings>(cancellationToken));
    }

    public async Task<OidcSettingsView> GetAsync(string requestOrigin, CancellationToken cancellationToken)
    {
        return await ViewOfAsync(await store.GetAsync<OidcSettings>(cancellationToken), requestOrigin, cancellationToken);
    }

    public Task<OidcSettingsView> DefaultsAsync(string requestOrigin, CancellationToken cancellationToken)
    {
        return ViewOfAsync(OidcSettings.Initial, requestOrigin, cancellationToken);
    }

    public async Task<OidcSettingsView> ChangeAsync(
        OidcSettingsChange change,
        string requestOrigin,
        CancellationToken cancellationToken)
    {
        OidcSettings current = await store.GetAsync<OidcSettings>(cancellationToken);
        OidcSettings settings = Validate(change, current.ClientSecret);
        await SaveAsync(settings, cancellationToken);
        return await ViewOfAsync(settings, requestOrigin, cancellationToken);
    }

    public async Task EnablePasswordLoginAsync(CancellationToken cancellationToken)
    {
        OidcSettings current = await store.GetAsync<OidcSettings>(cancellationToken);
        await SaveAsync(current with { PasswordLogin = true }, cancellationToken);
    }

    private async Task SaveAsync(OidcSettings settings, CancellationToken cancellationToken)
    {
        await store.SaveAsync(settings, cancellationToken);
        observer.Changed(settings);
    }

    private async Task<OidcSettingsView> ViewOfAsync(OidcSettings settings, string requestOrigin, CancellationToken cancellationToken)
    {
        string origin = await server.PublicOriginAsync(requestOrigin, cancellationToken);
        return new OidcSettingsView(
            settings.Issuer,
            settings.ClientId,
            settings.ClientSecret is not null,
            settings.Scopes,
            settings.UserClaim,
            settings.AutoCreateUsers,
            settings.PasswordLogin,
            OidcEndpoints.Callback(origin),
            OidcEndpoints.SignedOut(origin));
    }

    private static OidcSettings Validate(OidcSettingsChange change, string? currentSecret)
    {
        string? issuer = SettingsUrl.Normalize(change.Issuer, "issuer");
        string? clientId = Text(change.ClientId);
        if ((issuer is null) != (clientId is null))
        {
            throw new InvalidSettingsException("The issuer and the client id must be set together.");
        }

        OidcSettings settings = new(
            issuer,
            clientId,
            change.ClientSecret is null ? currentSecret : Text(change.ClientSecret),
            ScopesOf(change.Scopes),
            Required(change.UserClaim, "user claim"),
            change.AutoCreateUsers,
            change.PasswordLogin);
        return settings.AllowsPasswordLogin == settings.PasswordLogin
            ? settings
            : throw new InvalidSettingsException("The password login can be disabled only when OIDC is configured.");
    }

    private static List<string> ScopesOf(IReadOnlyList<string> scopes)
    {
        return scopes
            .SelectMany(scope => scope.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .Prepend(OidcSettings.OpenIdScope)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string Required(string value, string name)
    {
        return Text(value) ?? throw new InvalidSettingsException($"The {name} is required.");
    }

    private static string? Text(string? value)
    {
        string? trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
