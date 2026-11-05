using Guardou.Application.Files;

namespace Guardou.Application.Uploads;

internal static class UploadPath
{
    private static readonly char[] Separators = ['/', '\\'];

    public static IEnumerable<string> FolderNames(string? relativePath)
    {
        string[] segments = (relativePath ?? string.Empty).Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        return segments.SkipLast(1).Select(ItemPlacement.ValidName);
    }
}
