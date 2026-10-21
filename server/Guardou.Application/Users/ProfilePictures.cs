namespace Guardou.Application.Users;

public sealed class ProfilePictures(IUserAccountStore store, IProfilePictureStore pictures)
{
    public async Task ReplaceAsync(string id, Stream content, CancellationToken cancellationToken)
    {
        UserAccount account = await FindAsync(id);
        await pictures.SaveAsync(account.Id, content, cancellationToken);
    }

    public async Task DeleteAsync(string id)
    {
        UserAccount account = await FindAsync(id);
        pictures.Delete(account.Id);
    }

    private async Task<UserAccount> FindAsync(string id)
    {
        return await store.FindAsync(id) ?? throw new UserNotFoundException(id);
    }
}
