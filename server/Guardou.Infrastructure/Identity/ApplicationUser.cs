using Microsoft.AspNetCore.Identity;

namespace Guardou.Infrastructure.Identity;

internal sealed class ApplicationUser : IdentityUser
{
    public required string DisplayName { get; set; }
}
