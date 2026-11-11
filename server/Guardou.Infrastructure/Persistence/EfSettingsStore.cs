using System.Text.Json;
using Guardou.Application.Settings;
using Guardou.Domain.Authentication;
using Guardou.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfSettingsStore(AppDbContext context, SettingsSecrets secrets) : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private DbSet<StoredSetting> Settings => context.Set<StoredSetting>();

    public async Task<TSettings?> FindAsync<TSettings>(CancellationToken cancellationToken)
        where TSettings : class, ISettings<TSettings>
    {
        string key = TSettings.Key;
        StoredSetting? stored = await Settings.AsNoTracking().SingleOrDefaultAsync(setting => setting.Key == key, cancellationToken);
        return stored is null || JsonSerializer.Deserialize<TSettings>(stored.Value, JsonOptions) is not { } settings
            ? null
            : secrets.Unprotect(settings);
    }

    public async Task SaveAsync<TSettings>(TSettings settings, CancellationToken cancellationToken)
        where TSettings : class, ISettings<TSettings>
    {
        string key = TSettings.Key;
        string value = JsonSerializer.Serialize(secrets.Protect(settings), JsonOptions);
        StoredSetting? stored = await Settings.SingleOrDefaultAsync(setting => setting.Key == key, cancellationToken);
        if (stored is null)
        {
            Settings.Add(new StoredSetting(key, value));
        }
        else
        {
            stored.Change(value);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ProtectStoredSecretsAsync(CancellationToken cancellationToken)
    {
        StoredSetting? stored = await Settings.AsNoTracking().SingleOrDefaultAsync(setting => setting.Key == OidcSettings.Key, cancellationToken);
        if (stored is not null
            && JsonSerializer.Deserialize<OidcSettings>(stored.Value, JsonOptions) is { ClientSecret: { } secret } settings
            && !SettingsSecrets.IsProtected(secret))
        {
            await SaveAsync(settings, cancellationToken);
        }
    }
}
