using Guardou.Application.Settings;
using Guardou.Domain.Settings;

namespace Guardou.UnitTests.Settings;

internal sealed class FakeSettingsStore : ISettingsStore
{
    private readonly Dictionary<string, object> _settings = [];

    public Task<TSettings?> FindAsync<TSettings>(CancellationToken cancellationToken)
        where TSettings : class, ISettings<TSettings>
    {
        return Task.FromResult(_settings.TryGetValue(TSettings.Key, out object? settings) ? (TSettings)settings : null);
    }

    public Task SaveAsync<TSettings>(TSettings settings, CancellationToken cancellationToken)
        where TSettings : class, ISettings<TSettings>
    {
        _settings[TSettings.Key] = settings;
        return Task.CompletedTask;
    }
}
