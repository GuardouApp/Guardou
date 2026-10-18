namespace Guardou.Domain.Previews;

public static class PreviewSizes
{
    public static IReadOnlyList<PreviewSize> All { get; } = Enum.GetValues<PreviewSize>();

    public static int MaximumEdge(PreviewSize size)
    {
        return size switch
        {
            PreviewSize.Thumbnail => 400,
            PreviewSize.Large => 1920,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null),
        };
    }
}
