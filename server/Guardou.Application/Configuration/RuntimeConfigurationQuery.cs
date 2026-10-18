using Guardou.Application.Users;
using Guardou.Domain;
using Guardou.Domain.Users;

namespace Guardou.Application.Configuration;

public sealed class RuntimeConfigurationQuery(Registration registration)
{
    public async Task<RuntimeConfiguration> GetAsync()
    {
        return new RuntimeConfiguration(
            Product.Name,
            [AuthenticationMethod.Password],
            await registration.IsOpenAsync(),
            PasswordPolicy.Default);
    }
}
