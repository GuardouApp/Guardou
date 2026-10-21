using Guardou.Application.Users;

namespace Guardou.UnitTests.Users;

internal sealed class FakeProfilePictureStore : IProfilePictureStore
{
    public List<(string UserId, Uri Source)> Imports { get; } = [];

    public HashSet<string> Deleted { get; } = [];

    public bool Contains(string userId)
    {
        return Imports.Any(import => import.UserId == userId);
    }

    public ProfilePicture? TryOpen(string userId)
    {
        return null;
    }

    public Task ImportAsync(string userId, Uri source, CancellationToken cancellationToken)
    {
        Imports.Add((userId, source));
        return Task.CompletedTask;
    }

    public void Delete(string userId)
    {
        Deleted.Add(userId);
    }
}
