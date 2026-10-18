using Guardou.Domain.Tasks;

namespace Guardou.UnitTests.Tasks;

public sealed class QueuedTaskTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

    [Fact]
    public void FailsWithATruncatedErrorAndResets()
    {
        QueuedTask task = QueuedTask.Create(TaskKind.Preview, Guid.NewGuid(), TaskPriority.Normal, Now);

        task.Fail(new string('x', QueuedTask.MaximumErrorLength + 10), Now);
        int errorLength = task.Error?.Length ?? 0;
        TaskState failed = task.State;
        task.Reset(Now);

        Assert.Equal(TaskState.Failed, failed);
        Assert.Equal(QueuedTask.MaximumErrorLength, errorLength);
        Assert.Equal(TaskState.Pending, task.State);
        Assert.Null(task.Error);
    }
}
