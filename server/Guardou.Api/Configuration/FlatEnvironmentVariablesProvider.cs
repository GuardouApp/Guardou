namespace Guardou.Api.Configuration;

internal sealed class FlatEnvironmentVariablesProvider(IReadOnlyDictionary<string, string> keys) : ConfigurationProvider
{
    public override void Load()
    {
        Data = keys
            .Select(pair => (pair.Value, Value: Environment.GetEnvironmentVariable(pair.Key)))
            .Where(pair => !string.IsNullOrEmpty(pair.Value))
            .ToDictionary(pair => pair.Item1, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }
}
