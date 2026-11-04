namespace Guardou.Domain.Users;

public sealed record UserPreferences(ThemePreference Theme, Language Language, UploadConflictPolicy UploadConflictPolicy)
{
    public static UserPreferences Defaults { get; } = new(ThemePreference.System, Language.System, UploadConflictPolicy.Ask);
}
