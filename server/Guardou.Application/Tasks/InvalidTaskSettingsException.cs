namespace Guardou.Application.Tasks;

public sealed class InvalidTaskSettingsException(string message) : Exception(message);
