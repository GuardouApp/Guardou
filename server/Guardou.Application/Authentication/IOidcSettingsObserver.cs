using Guardou.Domain.Authentication;

namespace Guardou.Application.Authentication;

public interface IOidcSettingsObserver
{
    void Changed(OidcSettings settings);
}
