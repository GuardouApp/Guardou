using Guardou.Domain.Users;
using Guardou.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guardou.Infrastructure.Persistence;

internal sealed class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Ignore(user => user.EmailConfirmed);
        builder.Ignore(user => user.PhoneNumber);
        builder.Ignore(user => user.PhoneNumberConfirmed);
        builder.Ignore(user => user.TwoFactorEnabled);
        builder.Property(user => user.DisplayName).HasMaxLength(DisplayNameRule.MaximumLength);
        builder.Property(user => user.Email).IsRequired();
        builder.Property(user => user.NormalizedEmail).IsRequired();
        builder.HasIndex(user => user.NormalizedEmail).IsUnique();
    }
}
