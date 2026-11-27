namespace Guardou.Application.Updates;

internal static class ReleaseVersion
{
    private const char Prefix = 'v';
    private static readonly char[] Suffixes = ['-', '+'];

    public static string Normalize(string tag)
    {
        return tag.TrimStart(Prefix);
    }

    public static bool IsNewer(string candidate, string current)
    {
        return CoreOf(candidate) is { } candidateCore && CoreOf(current) is { } currentCore && candidateCore > currentCore;
    }

    private static Version? CoreOf(string version)
    {
        string normalized = Normalize(version);
        int suffix = normalized.IndexOfAny(Suffixes);
        string core = suffix < 0 ? normalized : normalized[..suffix];
        return Version.TryParse(core, out Version? parsed) ? parsed : null;
    }
}
