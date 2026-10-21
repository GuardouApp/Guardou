using Guardou.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Persistence;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Ignore<IdentityUserToken<string>>();
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
