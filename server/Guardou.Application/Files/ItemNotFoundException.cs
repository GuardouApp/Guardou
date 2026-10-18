namespace Guardou.Application.Files;

public sealed class ItemNotFoundException(string message) : Exception(message);
