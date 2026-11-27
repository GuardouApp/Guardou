using System.Reflection;

namespace Guardou.Domain;

public static class Product
{
    public const string Name = "Guardou";
    public const string Repository = "guardouapp/guardou";

    public static string Version { get; } =
        typeof(Product).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

    public static string CookieName(string purpose)
    {
        return $"{Name.ToLowerInvariant()}.{purpose}";
    }

    public static string ReleaseUrl(string version)
    {
        return $"https://github.com/{Repository}/releases/tag/v{version}";
    }
}
