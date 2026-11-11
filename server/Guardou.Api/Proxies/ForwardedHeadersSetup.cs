using IPNetwork = System.Net.IPNetwork;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace Guardou.Api.Proxies;

internal sealed class ForwardedHeadersSetup(IOptions<ProxyOptions> proxy) : IConfigureOptions<ForwardedHeadersOptions>
{
    public void Configure(ForwardedHeadersOptions options)
    {
        IReadOnlyList<IPNetwork> networks = TrustedNetworks.TryParse(proxy.Value.TrustedProxies) ?? [];
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();
        foreach (IPNetwork network in networks)
        {
            options.KnownIPNetworks.Add(network);
        }

        options.ForwardedHeaders = networks.Count == 0
            ? ForwardedHeaders.None
            : ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    }

    public static void Add(IServiceCollection services)
    {
        services.AddOptions<ProxyOptions>().BindConfiguration(ProxyOptions.Section).ValidateOnStart();
        services.AddSingleton<IValidateOptions<ProxyOptions>, ProxyOptionsValidation>();
        services.AddSingleton<IConfigureOptions<ForwardedHeadersOptions>, ForwardedHeadersSetup>();
    }
}
