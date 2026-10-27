using System.Globalization;
using System.Text.RegularExpressions;

namespace Guardou.Application.Search;

internal sealed partial class ModifiedFilter(DateTimeOffset now, TimeZoneInfo zone)
{
    private const string Today = "today";

    public Period Parse(string value)
    {
        if (value.Equals(Today, StringComparison.OrdinalIgnoreCase))
        {
            return new Period(StartOf(LocalDate()), null);
        }

        if (DaysPattern().Match(value) is { Success: true } days)
        {
            return new Period(now.AddDays(-Number(days.Groups["days"].Value)), null);
        }

        return value switch
        {
            ['>', .. string rest] => new Period(CalendarPeriod(rest).Before, null),
            ['<', .. string rest] => new Period(null, CalendarPeriod(rest).From),
            _ => CalendarPeriod(value),
        };
    }

    private Period CalendarPeriod(string value)
    {
        if (YearPattern().IsMatch(value))
        {
            DateOnly start = new(Number(value), 1, 1);
            return new Period(StartOf(start), StartOf(start.AddYears(1)));
        }

        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly day))
        {
            return new Period(StartOf(day), StartOf(day.AddDays(1)));
        }

        throw new InvalidSearchQueryException($"The modified filter '{value}' is not valid.");
    }

    private DateOnly LocalDate()
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
    }

    private DateTimeOffset StartOf(DateOnly date)
    {
        DateTime midnight = date.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(midnight, zone.GetUtcOffset(midnight));
    }

    private static int Number(string value)
    {
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int number) && number > 0
            ? number
            : throw new InvalidSearchQueryException($"The number '{value}' is not valid.");
    }

    [GeneratedRegex("^(?<days>[0-9]{1,5})d$", RegexOptions.IgnoreCase)]
    private static partial Regex DaysPattern();

    [GeneratedRegex("^[12][0-9]{3}$")]
    private static partial Regex YearPattern();
}

internal sealed record Period(DateTimeOffset? From, DateTimeOffset? Before);
