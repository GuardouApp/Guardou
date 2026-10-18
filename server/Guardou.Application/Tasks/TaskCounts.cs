namespace Guardou.Application.Tasks;

public sealed record TaskCounts(int Pending, int Running, int Failed)
{
    public static TaskCounts None { get; } = new(0, 0, 0);
}
