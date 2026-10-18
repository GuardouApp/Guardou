using Guardou.Api.Configuration;

namespace Guardou.IntegrationTests;

public sealed class ConfigurationTests
{
    [Fact]
    public void ReadsOnlyTheMappedEnvironmentVariables()
    {
        string variable = $"GUARDOU_TEST_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(variable, "4");
        try
        {
            FlatEnvironmentVariablesProvider provider = new(new Dictionary<string, string> { [variable] = "Tasks:Workers" });

            provider.Load();

            Assert.True(provider.TryGet("Tasks:Workers", out string? workers));
            Assert.Equal("4", workers);
            Assert.False(provider.TryGet("Tasks:Unknown", out _));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }
}
