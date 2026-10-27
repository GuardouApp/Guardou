using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Guardou.Infrastructure.Indexing;

internal static class IndexPath
{
    private const char Separator = '/';
    private const char AfterSeparator = '0';
    private const string IdFormat = "N";

    private static readonly MethodInfo StringCompare = typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)])
        ?? throw new InvalidOperationException("string.Compare(string, string) is missing.");

    private static readonly ConstantExpression Zero = Expression.Constant(0);

    public static string Of(string? parentPath, Guid id)
    {
        return $"{parentPath ?? Separator.ToString()}{id.ToString(IdFormat, CultureInfo.InvariantCulture)}{Separator}";
    }

    public static IReadOnlyList<Guid> Ids(string path)
    {
        return path.Split(Separator, StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => Guid.ParseExact(segment, IdFormat))
            .ToList();
    }

    public static IReadOnlyList<Guid> AncestorIds(string path)
    {
        return Ids(path).SkipLast(1).ToList();
    }

    public static bool Contains(string ancestorPath, string path)
    {
        return path.StartsWith(ancestorPath, StringComparison.Ordinal);
    }

    public static Expression<Func<IndexEntry, bool>> WithinAny(IEnumerable<string> paths)
    {
        ParameterExpression entry = Expression.Parameter(typeof(IndexEntry), "entry");
        MemberExpression path = Expression.Property(entry, nameof(IndexEntry.Path));
        Expression body = paths.Select(prefix => Within(path, prefix)).Aggregate<Expression>(Expression.OrElse);
        return Expression.Lambda<Func<IndexEntry, bool>>(body, entry);
    }

    private static BinaryExpression Within(MemberExpression path, string prefix)
    {
        string upperBound = prefix[..^1] + AfterSeparator;
        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(Compare(path, prefix), Zero),
            Expression.LessThan(Compare(path, upperBound), Zero));
    }

    private static MethodCallExpression Compare(MemberExpression path, string value)
    {
        return Expression.Call(StringCompare, path, Expression.Constant(value));
    }
}
