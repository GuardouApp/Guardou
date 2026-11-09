namespace Guardou.Application.Quotas;

public sealed class QuotaExceededException()
    : Exception("The storage quota is full.")
{
    public const string Code = "quotaExceeded";
}
