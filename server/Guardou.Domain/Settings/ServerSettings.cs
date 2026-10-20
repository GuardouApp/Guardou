namespace Guardou.Domain.Settings;

public sealed record ServerSettings(string? ExternalUrl) : ISettings<ServerSettings>
{
    public static string Key => "server";

    public static ServerSettings Initial { get; } = new(ExternalUrl: null);
}
