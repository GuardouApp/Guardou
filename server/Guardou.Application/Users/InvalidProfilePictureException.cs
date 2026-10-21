namespace Guardou.Application.Users;

public sealed class InvalidProfilePictureException(string message, Exception innerException) : Exception(message, innerException);
