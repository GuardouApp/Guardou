namespace Guardou.Domain;

public static class Product
{
    public const string Name = "Guardou";

    public static string CookieName(string purpose)
    {
        return $"{Name.ToLowerInvariant()}.{purpose}";
    }
}
