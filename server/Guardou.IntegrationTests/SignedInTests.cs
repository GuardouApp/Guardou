using Guardou.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public abstract class SignedInTests : IAsyncLifetime
{
    protected const string BobUsername = "bob";
    protected const string BobPassword = "Second-user-pass1";

    internal TestServerFactory Factory { get; } = new();

    protected HttpClient Client { get; private set; } = new();

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Factory.EnsureAdministratorAsync();
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<UserAdministration>()
            .SetAsync(BobUsername, "bob@example.com", BobPassword, administrator: false);
        Client = await SignInAsync(TestServerFactory.AdminUsername, TestServerFactory.AdminPassword);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected async Task RenewSessionAsync()
    {
        Client.Dispose();
        Client = await SignInAsync(TestServerFactory.AdminUsername, TestServerFactory.AdminPassword);
    }

    protected Task<HttpClient> SignInAsBobAsync()
    {
        return SignInAsync(BobUsername, BobPassword);
    }

    protected IReadOnlyList<string> BlobFiles()
    {
        return Directory.Exists(Factory.FilesDirectory)
            ? Directory.EnumerateFiles(Factory.FilesDirectory, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(Factory.FilesDirectory, path).Replace(Path.DirectorySeparatorChar, '/'))
                .ToList()
            : [];
    }

    private async Task<HttpClient> SignInAsync(string username, string password)
    {
        HttpClient client = Factory.CreateClient();
        await client.LoginAsync(username, password);
        return client;
    }
}
