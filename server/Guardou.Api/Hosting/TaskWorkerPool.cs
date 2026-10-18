using System.Collections.Concurrent;
using Guardou.Application.Tasks;
using Guardou.Domain.Tasks;

namespace Guardou.Api.Hosting;

internal sealed partial class TaskWorkerPool(
    IServiceScopeFactory scopeFactory,
    TaskSignal signal,
    ILogger<TaskWorkerPool> logger) : BackgroundService
{
    private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<TaskKind, int> _running = new();
    private readonly ConcurrentDictionary<Guid, Task> _tasks = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await UseScopeAsync(provider => provider.GetRequiredService<TaskRunner>().RecoverAsync(stoppingToken));
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!await TryStartNextAsync(stoppingToken))
                {
                    await signal.WaitAsync(IdleTimeout, stoppingToken);
                }
            }
        }
        finally
        {
            await Task.WhenAll(_tasks.Values).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private async Task<bool> TryStartNextAsync(CancellationToken cancellationToken)
    {
        ClaimedTask? claimed = null;
        await UseScopeAsync(async provider =>
        {
            IReadOnlyList<TaskConcurrency> limits = await provider.GetRequiredService<QueueSettings>().ConcurrencyAsync(cancellationToken);
            List<TaskKind> available = limits
                .Where(limit => _running.GetValueOrDefault(limit.Kind) < limit.Concurrency)
                .Select(limit => limit.Kind)
                .ToList();
            claimed = available.Count == 0
                ? null
                : await provider.GetRequiredService<TaskRunner>().ClaimNextAsync(available, cancellationToken);
        });
        if (claimed is null)
        {
            return false;
        }

        _running.AddOrUpdate(claimed.Kind, 1, (_, count) => count + 1);
        Task task = RunAsync(claimed, cancellationToken);
        _tasks[claimed.Id] = task;
        if (task.IsCompleted)
        {
            _tasks.TryRemove(claimed.Id, out _);
        }

        return true;
    }

    private async Task RunAsync(ClaimedTask claimed, CancellationToken cancellationToken)
    {
        await Task.Yield();
        try
        {
            await UseScopeAsync(provider => provider.GetRequiredService<TaskRunner>().RunAsync(claimed.Id, cancellationToken));
        }
        finally
        {
            _running.AddOrUpdate(claimed.Kind, 0, (_, count) => count - 1);
            _tasks.TryRemove(claimed.Id, out _);
            signal.Notify();
        }
    }

    private async Task UseScopeAsync(Func<IServiceProvider, Task> operation)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            await operation(scope.ServiceProvider);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogTaskFailed(exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The task queue failed.")]
    private partial void LogTaskFailed(Exception exception);
}
