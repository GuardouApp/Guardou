namespace Guardou.Application.Files;

public sealed class InvalidItemRequestException(string message) : Exception(message);
