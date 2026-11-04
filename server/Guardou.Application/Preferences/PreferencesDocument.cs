using Guardou.Domain.Users;

namespace Guardou.Application.Preferences;

public sealed record PreferencesDocument(ThemePreference? Theme, Language? Language, UploadConflictPolicy? UploadConflictPolicy)
{
    public static PreferencesDocument Of(UserPreferences preferences)
    {
        return new PreferencesDocument(preferences.Theme, preferences.Language, preferences.UploadConflictPolicy);
    }

    public UserPreferences MergeOver(UserPreferences defaults)
    {
        return new UserPreferences(
            Theme ?? defaults.Theme,
            Language ?? defaults.Language,
            UploadConflictPolicy ?? defaults.UploadConflictPolicy);
    }
}
