using Guardou.Application.Notifications;
using Guardou.Application.Settings;
using Guardou.Application.Users;
using Guardou.Domain;
using Guardou.Domain.Settings;

namespace Guardou.Application.Updates;

public sealed class UpdateCheck(
    EffectiveSettings<ServerSettings> settings,
    IReleaseFeed releases,
    Administrators administrators,
    NotificationCenter notifications)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (!(await settings.GetAsync(cancellationToken)).UpdateCheck)
        {
            return;
        }

        if (await releases.LatestVersionAsync(cancellationToken) is { } latest && ReleaseVersion.IsNewer(latest, Product.Version))
        {
            await notifications.UpdateAvailableAsync(await administrators.IdsAsync(), ReleaseVersion.Normalize(latest), cancellationToken);
        }
    }
}
