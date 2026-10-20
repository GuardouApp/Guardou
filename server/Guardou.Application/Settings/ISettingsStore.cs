using Guardou.Domain.Settings;

namespace Guardou.Application.Settings;

public interface ISettingsStore
{
    Task<TSettings> GetAsync<TSettings>(CancellationToken cancellationToken)
        where TSettings : class, ISettings<TSettings>;

    Task SaveAsync<TSettings>(TSettings settings, CancellationToken cancellationToken)
        where TSettings : class, ISettings<TSettings>;
}
