using Guardou.Domain.Users;

namespace Guardou.Application.Preferences;

public sealed class UserPreferencesService(IPreferencesStore store)
{
    public async Task<UserPreferences> GetAsync(string userId, CancellationToken cancellationToken)
    {
        PreferencesDocument? document = await store.FindAsync(userId, cancellationToken);
        return document?.MergeOver(UserPreferences.Defaults) ?? UserPreferences.Defaults;
    }

    public async Task<UserPreferences> ChangeAsync(string userId, UserPreferences preferences, CancellationToken cancellationToken)
    {
        await store.SaveAsync(userId, PreferencesDocument.Of(preferences), cancellationToken);
        return await GetAsync(userId, cancellationToken);
    }
}
