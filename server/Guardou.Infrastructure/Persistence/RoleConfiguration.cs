using Guardou.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guardou.Infrastructure.Persistence;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(new IdentityRole
        {
            Id = "2b0e7f4c-5d0a-4c55-9a43-6f0c1f2e8a11",
            Name = Roles.Admin,
            NormalizedName = Roles.Admin.ToUpperInvariant(),
            ConcurrencyStamp = "8f4d6c2e-1b3a-4e5f-9c7d-0a1b2c3d4e5f",
        });
    }
}
