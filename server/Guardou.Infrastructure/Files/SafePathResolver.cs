namespace Guardou.Infrastructure.Files;

internal static class SafePathResolver
{
    private static readonly StringComparison PathComparison =
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    public static string? Resolve(string root, string relativePath)
    {
        string trimmedRoot = Path.TrimEndingDirectorySeparator(root);
        string path = Path.GetFullPath(Path.Combine(trimmedRoot, relativePath));
        return IsInside(trimmedRoot, path) && !path.Equals(trimmedRoot, PathComparison) ? path : null;
    }

    public static bool IsInside(string root, string path)
    {
        string trimmedRoot = Path.TrimEndingDirectorySeparator(root);
        string trimmedPath = Path.TrimEndingDirectorySeparator(path);
        return trimmedPath.Equals(trimmedRoot, PathComparison)
            || trimmedPath.StartsWith(trimmedRoot + Path.DirectorySeparatorChar, PathComparison);
    }
}
