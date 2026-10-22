namespace Guardou.Application.Shares;

public sealed class ShareNotFoundException(string message) : Exception(message);
