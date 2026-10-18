using Guardou.Application.Files;

namespace Guardou.Api.Hosting;

internal sealed partial class TrashPurgeWorker(IServiceScopeFactory scopeFactory, ILogger<TrashPurgeWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(Interval);
        do
        {
            await PurgeAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<TrashService>().PurgeExpiredAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogPurgeFailed(exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The trash purge failed.")]
    private partial void LogPurgeFailed(Exception exception);
}
