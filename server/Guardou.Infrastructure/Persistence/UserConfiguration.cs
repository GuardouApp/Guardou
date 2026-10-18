using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guardou.Infrastructure.Persistence;

internal sealed class UserConfiguration : IEntityTypeConfiguration<IdentityUser>
{
    public void Configure(EntityTypeBuilder<IdentityUser> builder)
    {
        builder.Ignore(user => user.EmailConfirmed);
        builder.Ignore(user => user.PhoneNumber);
        builder.Ignore(user => user.PhoneNumberConfirmed);
        builder.Property(user => user.Email).IsRequired();
        builder.Property(user => user.NormalizedEmail).IsRequired();
        builder.HasIndex(user => user.NormalizedEmail).IsUnique();
    }
}
