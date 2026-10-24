using Guardou.Application.Users;
using Guardou.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Guardou.Infrastructure.Identity;

internal sealed class EfUserDirectory(AppDbContext context, IProfilePictureStore pictures) : IUserDirectory
{
    private const string EscapeCharacter = "\\";

    public async Task<IReadOnlyList<UserSummary>> SearchAsync(
        string query,
        string excludedId,
        int limit,
        CancellationToken cancellationToken)
    {
        string escaped = Escape(query);
        string prefix = escaped + "%";
        string contained = "%" + escaped + "%";
        List<ApplicationUser> users = await context.Users
            .Where(user => user.Id != excludedId)
            .Where(user => EF.Functions.Like(user.UserName, prefix, EscapeCharacter)
                || EF.Functions.Like(user.Email, prefix, EscapeCharacter)
                || EF.Functions.Like(user.DisplayName, contained, EscapeCharacter))
            .OrderBy(user => user.DisplayName)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return users.Select(ToSummary).ToList();
    }

    public async Task<IReadOnlyList<UserSummary>> FindAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken)
    {
        List<ApplicationUser> users = await context.Users.Where(user => ids.Contains(user.Id)).ToListAsync(cancellationToken);
        return users.Select(ToSummary).ToList();
    }

    private static string Escape(string value)
    {
        return value
            .Replace(EscapeCharacter, EscapeCharacter + EscapeCharacter, StringComparison.Ordinal)
            .Replace("%", EscapeCharacter + "%", StringComparison.Ordinal)
            .Replace("_", EscapeCharacter + "_", StringComparison.Ordinal);
    }

    private UserSummary ToSummary(ApplicationUser user)
    {
        return new UserSummary(user.Id, user.UserName ?? string.Empty, user.DisplayName, user.Email ?? string.Empty, pictures.Contains(user.Id));
    }
}
