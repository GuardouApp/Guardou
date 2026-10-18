namespace Guardou.IntegrationTests;

internal sealed class TestTimeProvider : TimeProvider
{
    private DateTimeOffset _now = DateTimeOffset.UtcNow;

    public override DateTimeOffset GetUtcNow()
    {
        return _now;
    }

    public void Advance(TimeSpan duration)
    {
        _now += duration;
    }
}
