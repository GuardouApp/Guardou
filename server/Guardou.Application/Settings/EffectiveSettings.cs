using Guardou.Domain.Settings;
using Microsoft.Extensions.Options;

namespace Guardou.Application.Settings;

public sealed class EffectiveSettings<TSettings>
    where TSettings : class, ISettings<TSettings>
{
    private readonly ISettingsStore _store;
    private readonly IReadOnlyList<SettingField<TSettings>> _fields;
    private readonly IReadOnlyList<(SettingField<TSettings> Field, SettingVariable<TSettings> Variable)> _locked;
    private readonly TSettings _environment;

    internal EffectiveSettings(ISettingsStore store, ISettingsSchema<TSettings> schema, IOptions<SettingsOverrides> overrides)
    {
        _store = store;
        _fields = schema.Fields;
        _locked = LockedFields(schema.Fields, overrides.Value).ToList();
        _environment = schema.FromEnvironment(overrides.Value);
    }

    public IReadOnlyList<LockedSetting> Locked => _locked.Select(locked => new LockedSetting(locked.Field.Name, locked.Variable.Name)).ToList();

    public TSettings Defaults => Apply(TSettings.Initial);

    public async Task<TSettings> GetAsync(CancellationToken cancellationToken)
    {
        return Apply(await StoredAsync(cancellationToken));
    }

    public Task<TSettings> StoredAsync(CancellationToken cancellationToken)
    {
        return _store.GetAsync<TSettings>(cancellationToken);
    }

    public TSettings Apply(TSettings settings)
    {
        return CopyLocked(settings, _environment);
    }

    public async Task SaveAsync(TSettings requested, CancellationToken cancellationToken)
    {
        await _store.SaveAsync(CopyLocked(requested, await StoredAsync(cancellationToken)), cancellationToken);
    }

    public async Task<IReadOnlyList<EffectiveSetting>> DescribeAsync(CancellationToken cancellationToken)
    {
        TSettings? stored = await _store.FindAsync<TSettings>(cancellationToken);
        TSettings effective = Apply(stored ?? TSettings.Initial);
        return _fields.Select(field => Describe(field, effective, stored is not null)).ToList();
    }

    private EffectiveSetting Describe(SettingField<TSettings> field, TSettings effective, bool stored)
    {
        bool locked = _locked.Any(entry => entry.Field == field);
        SettingSource source = locked ? SettingSource.Environment : stored ? SettingSource.Database : SettingSource.Default;
        return new EffectiveSetting(
            $"{TSettings.Key}.{field.Name}",
            field.Secret ? null : field.Text(effective),
            field.Secret,
            source,
            field.Variable?.Name);
    }

    private static IEnumerable<(SettingField<TSettings> Field, SettingVariable<TSettings> Variable)> LockedFields(
        IReadOnlyList<SettingField<TSettings>> fields,
        SettingsOverrides overrides)
    {
        foreach (SettingField<TSettings> field in fields)
        {
            if (field.Variable is { } variable && variable.IsSet(overrides))
            {
                yield return (field, variable);
            }
        }
    }

    private TSettings CopyLocked(TSettings target, TSettings source)
    {
        return _locked.Aggregate(target, (settings, locked) => locked.Variable.Copy(settings, source));
    }
}
