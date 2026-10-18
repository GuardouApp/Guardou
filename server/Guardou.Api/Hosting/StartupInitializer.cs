using Guardou.Application.Persistence;

namespace Guardou.Api.Hosting;

internal sealed class StartupInitializer(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>().MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
