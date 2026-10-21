using Guardou.Application.Users;

namespace Guardou.UnitTests.Users;

public sealed class ProfilePicturesTests
{
    private readonly FakeUserAccountStore _store = new();
    private readonly FakeProfilePictureStore _pictures = new();

    [Fact]
    public async Task ReplaceSavesThePictureOfAnExistingUser()
    {
        UserAccount bob = await CreateBobAsync();
        using MemoryStream content = new([1, 2, 3]);

        await new ProfilePictures(_store, _pictures).ReplaceAsync(bob.Id, content, TestContext.Current.CancellationToken);

        Assert.Equal([bob.Id], _pictures.Saved);
    }

    [Fact]
    public async Task ReplaceRefusesAMissingUser()
    {
        using MemoryStream content = new([1, 2, 3]);

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => new ProfilePictures(_store, _pictures).ReplaceAsync("missing", content, TestContext.Current.CancellationToken));
        Assert.Empty(_pictures.Saved);
    }

    [Fact]
    public async Task DeleteRemovesThePictureOfAnExistingUser()
    {
        UserAccount bob = await CreateBobAsync();

        await new ProfilePictures(_store, _pictures).DeleteAsync(bob.Id);

        Assert.Equal([bob.Id], _pictures.Deleted);
    }

    private async Task<UserAccount> CreateBobAsync()
    {
        await _store.CreateAsync(new UserProfile("bob", "Bob", "bob@example.com", IsAdmin: false), "password");
        return _store.ByUsername("bob");
    }
}
