using Guardou.Domain.Tasks;

namespace Guardou.UnitTests.Tasks;

public sealed class TaskQueueSettingsTests
{
    [Fact]
    public void UsesTheDefaultConcurrency()
    {
        Assert.Equal(TaskQueueSettings.DefaultConcurrency, TaskQueueSettings.Create(TaskKind.Preview).Concurrency);
    }

    [Fact]
    public void ChangesTheConcurrency()
    {
        TaskQueueSettings settings = TaskQueueSettings.Create(TaskKind.Preview);

        settings.ChangeConcurrency(TaskQueueSettings.MaximumConcurrency);

        Assert.Equal(TaskQueueSettings.MaximumConcurrency, settings.Concurrency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(17)]
    public void RefusesAConcurrencyOutOfRange(int concurrency)
    {
        TaskQueueSettings settings = TaskQueueSettings.Create(TaskKind.Preview);

        Assert.Throws<ArgumentOutOfRangeException>(() => settings.ChangeConcurrency(concurrency));
    }
}
