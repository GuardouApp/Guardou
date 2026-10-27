using System.Globalization;
using System.Text.RegularExpressions;

namespace Guardou.Application.Search;

internal static partial class SizeFilter
{
    private static readonly Dictionary<string, long> Units = new(StringComparer.OrdinalIgnoreCase)
    {
        ["b"] = 1,
        ["kb"] = 1L << 10,
        ["mb"] = 1L << 20,
        ["gb"] = 1L << 30,
        ["tb"] = 1L << 40,
    };

    public static SizeRange Parse(string value)
    {
        Match match = SizePattern().Match(value);
        if (!match.Success)
        {
            throw new InvalidSearchQueryException($"The size filter '{value}' is not valid.");
        }

        decimal number = decimal.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
        string unit = match.Groups["unit"].Value;
        long bytes = (long)(number * Units[unit.Length == 0 ? "b" : unit]);
        return match.Groups["operator"].Value == ">" ? new SizeRange(bytes, null) : new SizeRange(null, bytes);
    }

    [GeneratedRegex("^(?<operator>[<>])(?<number>[0-9]{1,6}(\\.[0-9]{1,3})?)(?<unit>b|kb|mb|gb|tb)?$", RegexOptions.IgnoreCase)]
    private static partial Regex SizePattern();
}

internal sealed record SizeRange(long? LargerThan, long? SmallerThan);
