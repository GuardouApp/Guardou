using Microsoft.Extensions.Options;

namespace Guardou.Api.Proxies;

internal sealed class ProxyOptionsValidation : IValidateOptions<ProxyOptions>
{
    public ValidateOptionsResult Validate(string? name, ProxyOptions options)
    {
        return TrustedNetworks.TryParse(options.TrustedProxies) is null
            ? ValidateOptionsResult.Fail(
                $"{ProxyOptions.TrustedProxiesVariable} must list IP addresses or CIDR networks separated by spaces.")
            : ValidateOptionsResult.Success;
    }
}
