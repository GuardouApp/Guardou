namespace Guardou.Application.Uploads;

public sealed class UploadInProgressException(string name)
    : Exception($"Another upload of '{name}' to the same destination is in progress.")
{
    public const string Code = "uploadInProgress";
}
