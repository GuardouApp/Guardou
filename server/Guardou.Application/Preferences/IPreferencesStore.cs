namespace Guardou.Application.Preferences;

public interface IPreferencesStore
{
    Task<PreferencesDocument?> FindAsync(string userId, CancellationToken cancellationToken);

    Task SaveAsync(string userId, PreferencesDocument document, CancellationToken cancellationToken);
}
