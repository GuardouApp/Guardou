using System.Globalization;

namespace Guardou.Domain.Files;

public static class StorageKey
{
    private const int MaximumExtensionLength = 16;
    private const char Separator = '/';

    public static string For(string ownerId, Guid id, string name, DateTimeOffset date)
    {
        DateTime day = date.UtcDateTime;
        return string.Join(
            Separator,
            ownerId,
            day.ToString("yyyy", CultureInfo.InvariantCulture),
            day.ToString("MM", CultureInfo.InvariantCulture),
            day.ToString("dd", CultureInfo.InvariantCulture),
            FileName(id, name));
    }

    private static string FileName(Guid id, string name)
    {
        string extension = ItemName.Extension(name).ToLowerInvariant();
        bool safe = extension.Length is > 0 and <= MaximumExtensionLength && extension.All(char.IsAsciiLetterOrDigit);
        return safe ? $"{id:N}.{extension}" : id.ToString("N");
    }
}
