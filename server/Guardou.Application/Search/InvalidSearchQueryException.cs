namespace Guardou.Application.Search;

public sealed class InvalidSearchQueryException(string message) : Exception(message);
