using Guardou.Application.Authentication;
using Guardou.Domain.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Guardou.Infrastructure.Identity.Oidc;

internal sealed class OidcSettingsSnapshot : IOidcSettingsObserver, IOptionsChangeTokenSource<OpenIdConnectOptions>, IDisposable
{
    private readonly Lock _gate = new();
    private volatile OidcSettings _current = OidcSettings.Initial;
    private CancellationTokenSource _changes = new();

    public OidcSettings Current => _current;

    public string Name => OidcSettings.Provider;

    public IChangeToken GetChangeToken()
    {
        lock (_gate)
        {
            return new CancellationChangeToken(_changes.Token);
        }
    }

    public void Changed(OidcSettings settings)
    {
        CancellationTokenSource previous;
        lock (_gate)
        {
            _current = settings;
            previous = _changes;
            _changes = new CancellationTokenSource();
        }

        previous.Cancel();
        previous.Dispose();
    }

    public void Dispose()
    {
        _changes.Dispose();
    }
}
