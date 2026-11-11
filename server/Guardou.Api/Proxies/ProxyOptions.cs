namespace Guardou.Api.Proxies;

internal sealed class ProxyOptions
{
    public const string Section = "Proxy";
    public const string TrustedProxiesVariable = "TRUSTED_PROXIES";

    public string? TrustedProxies { get; set; }
}
