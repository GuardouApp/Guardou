using System.Text;

namespace Guardou.Application.Search;

internal static class SearchTokenizer
{
    private const char Quote = '"';

    public static IEnumerable<SearchToken> Tokenize(string text)
    {
        StringBuilder current = new();
        bool quoted = false;
        bool wasQuoted = false;
        foreach (char character in text)
        {
            if (character == Quote)
            {
                quoted = !quoted;
                wasQuoted = true;
            }
            else if (char.IsWhiteSpace(character) && !quoted)
            {
                if (TakeToken(current, wasQuoted) is { } token)
                {
                    yield return token;
                }

                wasQuoted = false;
            }
            else
            {
                current.Append(character);
            }
        }

        if (TakeToken(current, wasQuoted) is { } last)
        {
            yield return last;
        }
    }

    private static SearchToken? TakeToken(StringBuilder current, bool quoted)
    {
        string value = current.ToString().Trim();
        current.Clear();
        return value.Length == 0 ? null : new SearchToken(value, quoted);
    }
}

internal sealed record SearchToken(string Value, bool Quoted);
