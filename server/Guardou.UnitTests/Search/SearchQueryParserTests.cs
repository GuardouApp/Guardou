using Guardou.Application.Search;
using Guardou.Domain.Files;

namespace Guardou.UnitTests.Search;

public sealed class SearchQueryParserTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 15, 30, 0, TimeSpan.Zero);

    private readonly SearchQueryParser _parser = new(new FixedTimeProvider(Now));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivesAnEmptyQueryForBlankText(string? text)
    {
        Assert.True(_parser.Parse(text).IsEmpty);
    }

    [Fact]
    public void SplitsWordsIntoTerms()
    {
        Assert.Equal(["annual", "report"], _parser.Parse("  annual   report ").Terms);
    }

    [Fact]
    public void KeepsAQuotedPhraseAsOneTerm()
    {
        Assert.Equal(["annual report", "2026"], _parser.Parse("\"annual report\" 2026").Terms);
    }

    [Fact]
    public void KeepsAQuotedFilterAsText()
    {
        SearchQuery query = _parser.Parse("\"type:image\"");

        Assert.Equal(["type:image"], query.Terms);
        Assert.Empty(query.Types);
    }

    [Fact]
    public void KeepsUnknownKeysAsText()
    {
        Assert.Equal(["note:draft"], _parser.Parse("note:draft").Terms);
    }

    [Fact]
    public void CollectsTypes()
    {
        SearchQuery query = _parser.Parse("type:image TYPE:Video holiday");

        Assert.Equal([ItemType.Image, ItemType.Video], query.Types.Order());
        Assert.Equal(["holiday"], query.Terms);
    }

    [Theory]
    [InlineData("type:pictures")]
    [InlineData("type:1")]
    [InlineData("type:")]
    public void RefusesAnUnknownType(string text)
    {
        Assert.Throws<InvalidSearchQueryException>(() => _parser.Parse(text));
    }

    [Fact]
    public void ParsesModifiedToday()
    {
        SearchQuery query = _parser.Parse("modified:today");

        Assert.Equal(new DateTimeOffset(2026, 10, 27, 0, 0, 0, TimeSpan.Zero), query.ModifiedFrom);
        Assert.Null(query.ModifiedBefore);
    }

    [Fact]
    public void ParsesModifiedDays()
    {
        Assert.Equal(Now.AddDays(-7), _parser.Parse("modified:7d").ModifiedFrom);
    }

    [Fact]
    public void ParsesAModifiedYear()
    {
        SearchQuery query = _parser.Parse("modified:2025");

        Assert.Equal(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), query.ModifiedFrom);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), query.ModifiedBefore);
    }

    [Fact]
    public void ParsesAModifiedDay()
    {
        SearchQuery query = _parser.Parse("modified:2026-02-28");

        Assert.Equal(new DateTimeOffset(2026, 2, 28, 0, 0, 0, TimeSpan.Zero), query.ModifiedFrom);
        Assert.Equal(new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero), query.ModifiedBefore);
    }

    [Fact]
    public void ParsesModifiedAfterAndBefore()
    {
        SearchQuery query = _parser.Parse("modified:>2025-12-31 modified:<2027");

        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), query.ModifiedFrom);
        Assert.Equal(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero), query.ModifiedBefore);
    }

    [Fact]
    public void IntersectsModifiedFilters()
    {
        SearchQuery query = _parser.Parse("modified:2026 modified:30d modified:<2026-10-20");

        Assert.Equal(Now.AddDays(-30), query.ModifiedFrom);
        Assert.Equal(new DateTimeOffset(2026, 10, 20, 0, 0, 0, TimeSpan.Zero), query.ModifiedBefore);
    }

    [Theory]
    [InlineData("modified:yesterday")]
    [InlineData("modified:0d")]
    [InlineData("modified:2026-13-01")]
    [InlineData("modified:2026-03")]
    [InlineData("modified:>")]
    public void RefusesAnInvalidModifiedFilter(string text)
    {
        Assert.Throws<InvalidSearchQueryException>(() => _parser.Parse(text));
    }

    [Theory]
    [InlineData("size:>10", 10L, null)]
    [InlineData("size:>1KB", 1024L, null)]
    [InlineData("size:<1.5mb", null, 1572864L)]
    [InlineData("size:>2GB", 2147483648L, null)]
    [InlineData("size:<1TB", null, 1099511627776L)]
    public void ParsesSizes(string text, long? larger, long? smaller)
    {
        SearchQuery query = _parser.Parse(text);

        Assert.Equal(larger, query.LargerThan);
        Assert.Equal(smaller, query.SmallerThan);
    }

    [Fact]
    public void IntersectsSizes()
    {
        SearchQuery query = _parser.Parse("size:>1MB size:>2MB size:<1GB size:<3GB");

        Assert.Equal(2L << 20, query.LargerThan);
        Assert.Equal(1L << 30, query.SmallerThan);
    }

    [Theory]
    [InlineData("size:10MB")]
    [InlineData("size:>MB")]
    [InlineData("size:>10PB")]
    [InlineData("size:>-1")]
    public void RefusesAnInvalidSize(string text)
    {
        Assert.Throws<InvalidSearchQueryException>(() => _parser.Parse(text));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        public override DateTimeOffset GetUtcNow()
        {
            return now;
        }
    }
}
