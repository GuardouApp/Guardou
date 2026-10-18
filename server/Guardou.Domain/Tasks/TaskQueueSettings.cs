namespace Guardou.Domain.Tasks;

public sealed class TaskQueueSettings
{
    public const int DefaultConcurrency = 2;
    public const int MinimumConcurrency = 1;
    public const int MaximumConcurrency = 16;

    private TaskQueueSettings(TaskKind kind, int concurrency)
    {
        Kind = kind;
        Concurrency = concurrency;
    }

    public TaskKind Kind { get; private set; }

    public int Concurrency { get; private set; }

    public static TaskQueueSettings Create(TaskKind kind)
    {
        return new TaskQueueSettings(kind, DefaultConcurrency);
    }

    public static bool IsValidConcurrency(int concurrency)
    {
        return concurrency is >= MinimumConcurrency and <= MaximumConcurrency;
    }

    public void ChangeConcurrency(int concurrency)
    {
        Concurrency = IsValidConcurrency(concurrency)
            ? concurrency
            : throw new ArgumentOutOfRangeException(nameof(concurrency), concurrency, null);
    }
}
