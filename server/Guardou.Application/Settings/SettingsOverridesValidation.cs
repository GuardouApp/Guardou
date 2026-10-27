using Guardou.Domain.Authentication;
using Guardou.Domain.Settings;
using Microsoft.Extensions.Options;

namespace Guardou.Application.Settings;

internal sealed class SettingsOverridesValidation(
    ISettingsSchema<ServerSettings> server,
    ISettingsSchema<OidcSettings> oidc) : IValidateOptions<SettingsOverrides>
{
    public ValidateOptionsResult Validate(string? name, SettingsOverrides options)
    {
        try
        {
            server.FromEnvironment(options);
            oidc.FromEnvironment(options);
            return ValidateOptionsResult.Success;
        }
        catch (InvalidSettingsException exception)
        {
            return ValidateOptionsResult.Fail(exception.Message);
        }
    }
}
