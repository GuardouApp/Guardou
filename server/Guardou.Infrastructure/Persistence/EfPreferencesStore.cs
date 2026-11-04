using System.Text.Json;
using System.Text.Json.Serialization;
using Guardou.Application.Preferences;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfPreferencesStore(AppDbContext context) : IPreferencesStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private DbSet<StoredPreferences> Preferences => context.Set<StoredPreferences>();

    public async Task<PreferencesDocument?> FindAsync(string userId, CancellationToken cancellationToken)
    {
        StoredPreferences? stored = await Preferences.AsNoTracking().SingleOrDefaultAsync(preferences => preferences.UserId == userId, cancellationToken);
        return stored is null ? null : JsonSerializer.Deserialize<PreferencesDocument>(stored.Value, JsonOptions);
    }

    public async Task SaveAsync(string userId, PreferencesDocument document, CancellationToken cancellationToken)
    {
        string value = JsonSerializer.Serialize(document, JsonOptions);
        StoredPreferences? stored = await Preferences.SingleOrDefaultAsync(preferences => preferences.UserId == userId, cancellationToken);
        if (stored is null)
        {
            Preferences.Add(new StoredPreferences(userId, value));
        }
        else
        {
            stored.Change(value);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
