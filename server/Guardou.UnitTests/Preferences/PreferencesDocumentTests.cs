using Guardou.Application.Preferences;
using Guardou.Domain.Users;

namespace Guardou.UnitTests.Preferences;

public sealed class PreferencesDocumentTests
{
    private static readonly UserPreferences Defaults = new(ThemePreference.Dark, Language.Pt, UploadConflictPolicy.Rename);

    [Fact]
    public void AnEmptyDocumentGivesTheDefaults()
    {
        UserPreferences merged = new PreferencesDocument(null, null, null).MergeOver(Defaults);

        Assert.Equal(Defaults, merged);
    }

    [Fact]
    public void StoredValuesReplaceTheDefaults()
    {
        UserPreferences merged = new PreferencesDocument(ThemePreference.Light, null, UploadConflictPolicy.Skip).MergeOver(Defaults);

        Assert.Equal(new UserPreferences(ThemePreference.Light, Language.Pt, UploadConflictPolicy.Skip), merged);
    }

    [Fact]
    public void AFullDocumentKeepsAllValues()
    {
        UserPreferences preferences = new(ThemePreference.System, Language.PtBr, UploadConflictPolicy.Replace);

        Assert.Equal(preferences, PreferencesDocument.Of(preferences).MergeOver(Defaults));
    }
}
