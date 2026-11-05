using Guardou.Application.Uploads;

namespace Guardou.Api.Hosting;

internal sealed partial class UploadCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<UploadCleanupWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(Interval);
        do
        {
            await RemoveExpiredAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RemoveExpiredAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<UploadCleanup>().RemoveExpiredAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogCleanupFailed(exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The removal of expired uploads failed.")]
    private partial void LogCleanupFailed(Exception exception);
}
