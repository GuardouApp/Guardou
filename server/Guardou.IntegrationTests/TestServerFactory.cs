using Guardou.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Guardou.IntegrationTests;

public sealed class TestServerFactory : WebApplicationFactory<Program>
{
    public const string AdminUsername = "admin";
    public const string AdminEmail = "admin@example.com";
    public const string AdminPassword = "Integration-pass1";

    private readonly string _dataDir = Path.Combine(Path.GetTempPath(), "guardou-tests", Guid.NewGuid().ToString("N"));

    internal string FilesDirectory => Path.Combine(_dataDir, "data");

    internal string CacheDirectory => Path.Combine(_dataDir, "cache");

    internal TestTimeProvider Time { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string webRoot = Directory.CreateDirectory(Path.Combine(_dataDir, "wwwroot")).FullName;
        File.WriteAllText(Path.Combine(webRoot, "index.html"), "<!doctype html>");
        builder.UseWebRoot(webRoot);
        builder.ConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Singleton(new StoragePaths(FilesDirectory, Path.Combine(_dataDir, "db"), CacheDirectory)));
            services.Replace(ServiceDescriptor.Singleton<TimeProvider>(Time));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        SqliteConnection.ClearAllPools();
        Directory.Delete(_dataDir, recursive: true);
    }
}
