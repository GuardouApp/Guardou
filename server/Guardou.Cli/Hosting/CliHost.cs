using System.CommandLine;
using Guardou.Application;
using Guardou.Application.Persistence;
using Guardou.Application.Users;
using Guardou.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Guardou.Cli.Hosting;

internal static class CliHost
{
    public static async Task<int> ExecuteAsync(
        ParseResult parseResult,
        Func<UserAdministration, Task<UserAccountResult>> operation,
        CancellationToken cancellationToken)
    {
        using IHost host = Build();
        await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>().MigrateAsync(cancellationToken);

        UserAccountResult result = await operation(scope.ServiceProvider.GetRequiredService<UserAdministration>());
        foreach (UserAccountError error in result.Errors)
        {
            await parseResult.InvocationConfiguration.Error.WriteLineAsync(error.Description);
        }

        return result.Succeeded ? 0 : 1;
    }

    private static IHost Build()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = AppContext.BaseDirectory,
        });
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();
        return builder.Build();
    }
}
