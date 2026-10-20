namespace Guardou.Application.Settings;

public sealed class InvalidSettingsException(string message) : Exception(message);
