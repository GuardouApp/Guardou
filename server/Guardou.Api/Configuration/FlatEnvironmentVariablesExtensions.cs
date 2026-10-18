namespace Guardou.Api.Configuration;

internal static class FlatEnvironmentVariablesExtensions
{
    public static IConfigurationBuilder AddFlatEnvironmentVariables(this IConfigurationBuilder builder)
    {
        return builder.Add(new FlatEnvironmentVariablesSource(EnvironmentVariableMap.Keys));
    }
}
