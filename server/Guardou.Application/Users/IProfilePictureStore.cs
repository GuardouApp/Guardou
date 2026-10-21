namespace Guardou.Application.Users;

public interface IProfilePictureStore
{
    bool Contains(string userId);

    ProfilePicture? TryOpen(string userId);

    Task ImportAsync(string userId, Uri source, CancellationToken cancellationToken);

    void Delete(string userId);
}
