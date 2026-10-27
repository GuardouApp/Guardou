using Guardou.Domain.Settings;

namespace Guardou.Application.Settings;

internal static class SettingsStoreExtensions
{
    public static async Task<TSettings> GetAsync<TSettings>(this ISettingsStore store, CancellationToken cancellationToken)
        where TSettings : class, ISettings<TSettings>
    {
        return await store.FindAsync<TSettings>(cancellationToken) ?? TSettings.Initial;
    }
}
