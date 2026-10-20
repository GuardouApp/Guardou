using Guardou.Domain.Settings;

namespace Guardou.Application.Settings;

public sealed class ServerSettingsAdministration(ISettingsStore store)
{
    public Task<ServerSettings> GetAsync(CancellationToken cancellationToken)
    {
        return store.GetAsync<ServerSettings>(cancellationToken);
    }

    public async Task<ServerSettings> ChangeAsync(ServerSettings settings, CancellationToken cancellationToken)
    {
        ServerSettings normalized = new(SettingsUrl.Normalize(settings.ExternalUrl, "external URL"));
        await store.SaveAsync(normalized, cancellationToken);
        return normalized;
    }

    public async Task<string> PublicOriginAsync(string requestOrigin, CancellationToken cancellationToken)
    {
        return (await GetAsync(cancellationToken)).ExternalUrl ?? requestOrigin;
    }
}
