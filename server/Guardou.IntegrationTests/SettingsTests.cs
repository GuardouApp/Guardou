using System.Net;
using System.Net.Http.Json;
using Guardou.Application.Tasks;
using Guardou.Domain.Tasks;

namespace Guardou.IntegrationTests;

public sealed class SettingsTests : SignedInTests
{
    [Fact]
    public async Task ReadsTheDefaultConcurrency()
    {
        IReadOnlyList<TaskConcurrency> settings = await GetTaskSettingsAsync(Client);

        Assert.Equal([new TaskConcurrency(TaskKind.Preview, TaskQueueSettings.DefaultConcurrency)], settings);
    }

    [Fact]
    public async Task ChangesTheConcurrency()
    {
        HttpResponseMessage response = await PutTaskSettingsAsync([new TaskConcurrency(TaskKind.Preview, 5)]);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([new TaskConcurrency(TaskKind.Preview, 5)], await GetTaskSettingsAsync(Client));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(17)]
    public async Task RefusesAConcurrencyOutOfRange(int concurrency)
    {
        HttpResponseMessage response = await PutTaskSettingsAsync([new TaskConcurrency(TaskKind.Preview, concurrency)]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefusesUsersWithoutTheAdminType()
    {
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage response = await bob.GetAsync("/api/settings/tasks", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<IReadOnlyList<TaskConcurrency>> GetTaskSettingsAsync(HttpClient client)
    {
        return await client.GetFromJsonAsync<IReadOnlyList<TaskConcurrency>>("/api/settings/tasks", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The task settings are missing.");
    }

    private Task<HttpResponseMessage> PutTaskSettingsAsync(IReadOnlyList<TaskConcurrency> settings)
    {
        return Client.PutAsJsonAsync("/api/settings/tasks", settings, HttpClientExtensions.JsonOptions, CancellationToken);
    }
}
