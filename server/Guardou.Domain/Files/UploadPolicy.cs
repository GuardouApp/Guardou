namespace Guardou.Domain.Files;

public static class UploadPolicy
{
    public const int DefaultConcurrency = 3;
    public const int MinimumConcurrency = 1;
    public const int MaximumConcurrency = 6;
    public const int DefaultChunkSizeMb = 64;
    public const int MinimumChunkSizeMb = 1;
    public const int MaximumChunkSizeMb = 90;

    public static bool IsValidConcurrency(int concurrency)
    {
        return concurrency is >= MinimumConcurrency and <= MaximumConcurrency;
    }

    public static bool IsValidChunkSize(int chunkSizeMb)
    {
        return chunkSizeMb is >= MinimumChunkSizeMb and <= MaximumChunkSizeMb;
    }
}
