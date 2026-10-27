using Guardou.Application.Settings;
using Guardou.Domain.Authentication;
using Guardou.Domain.Settings;

namespace Guardou.Application.Configuration;

public sealed class EffectiveConfigurationQuery(
    EffectiveSettings<ServerSettings> server,
    EffectiveSettings<OidcSettings> oidc)
{
    public async Task<IReadOnlyList<EffectiveSetting>> GetAsync(CancellationToken cancellationToken)
    {
        return [.. await server.DescribeAsync(cancellationToken), .. await oidc.DescribeAsync(cancellationToken)];
    }
}
