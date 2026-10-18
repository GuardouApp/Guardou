namespace Guardou.Api.Configuration;

internal sealed class FlatEnvironmentVariablesSource(IReadOnlyDictionary<string, string> keys) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new FlatEnvironmentVariablesProvider(keys);
    }
}
