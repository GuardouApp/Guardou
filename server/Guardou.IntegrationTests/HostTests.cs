using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Configuration;
using Guardou.Domain;
using Guardou.Domain.Users;

namespace Guardou.IntegrationTests;

public sealed class HostTests(TestServerFactory factory) : IClassFixture<TestServerFactory>
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ConfigurationIsPublic()
    {
        RuntimeConfiguration? configuration = await factory.CreateClient()
            .GetFromJsonAsync<RuntimeConfiguration>("/api/config", HttpClientExtensions.JsonOptions, CancellationToken);

        Assert.NotNull(configuration);
        Assert.Equal(Product.Name, configuration.ApplicationName);
        Assert.Equal([AuthenticationMethod.Password], configuration.AuthenticationMethods);
        Assert.True(configuration.RegistrationOpen);
        Assert.Equal(PasswordPolicy.Default, configuration.PasswordPolicy);
    }

    [Fact]
    public async Task HealthIsReported()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync("/health", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnknownApiRoutesReturnProblemDetails()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync("/api/missing", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
