using Guardou.Application.Updates;

namespace Guardou.Api.Hosting;

internal sealed partial class UpdateCheckWorker(IServiceScopeFactory scopeFactory, ILogger<UpdateCheckWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(Interval);
        do
        {
            await CheckAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<UpdateCheck>().RunAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogCheckFailed(exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "The update check failed.")]
    private partial void LogCheckFailed(Exception exception);
}
