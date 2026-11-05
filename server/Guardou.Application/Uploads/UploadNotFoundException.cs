namespace Guardou.Application.Uploads;

public sealed class UploadNotFoundException(string id) : Exception($"The upload '{id}' does not exist.");
