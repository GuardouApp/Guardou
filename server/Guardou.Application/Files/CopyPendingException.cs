namespace Guardou.Application.Files;

public sealed class CopyPendingException() : Exception("The copy of the file is not complete. Try again later.")
{
    public const string Code = "copyPending";
}
