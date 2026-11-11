using System.Net.Http.Json;
using Guardou.Application.Authentication;
using Guardou.Application.Persistence;
using Guardou.Application.Settings;
using Guardou.Domain.Authentication;
using Guardou.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class OidcSecretTests : SignedInTests
{
    private const string Secret = "client-secret-value";

    [Fact]
    public async Task EncryptsTheClientSecretInTheDatabase()
    {
        OidcSettingsChange change = new(
            Issuer: "https://id.example.com",
            ClientId: "guardou",
            ClientSecret: Secret,
            Scopes: ["openid"],
            UserClaim: "preferred_username",
            AutoCreateUsers: true,
            PasswordLogin: true);

        (await Client.PutAsJsonAsync("/api/settings/oidc", change, HttpClientExtensions.JsonOptions, CancellationToken)).EnsureSuccessStatusCode();

        Assert.DoesNotContain(Secret, await StoredValueAsync(), StringComparison.Ordinal);
        Assert.Equal(Secret, (await FindSettingsAsync())?.ClientSecret);
    }

    [Fact]
    public async Task EncryptsAPlainSecretAtStartup()
    {
        await using (AsyncServiceScope scope = Factory.Services.CreateAsyncScope())
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Set<StoredSetting>().Add(new StoredSetting(OidcSettings.Key, $$"""{"clientSecret":"{{Secret}}","scopes":["openid"],"userClaim":"sub"}"""));
            await context.SaveChangesAsync(CancellationToken);
            await scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>().MigrateAsync(CancellationToken);
        }

        Assert.DoesNotContain(Secret, await StoredValueAsync(), StringComparison.Ordinal);
        Assert.Equal(Secret, (await FindSettingsAsync())?.ClientSecret);
    }

    private async Task<string> StoredValueAsync()
    {
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        StoredSetting setting = await context.Set<StoredSetting>().SingleAsync(stored => stored.Key == OidcSettings.Key, CancellationToken);
        return setting.Value;
    }

    private async Task<OidcSettings?> FindSettingsAsync()
    {
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISettingsStore>().FindAsync<OidcSettings>(CancellationToken);
    }
}
