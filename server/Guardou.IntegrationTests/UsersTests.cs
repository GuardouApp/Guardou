using System.Net;
using System.Net.Http.Json;
using Guardou.Api.Models;
using Guardou.Application.Files;
using Guardou.Application.Users;

namespace Guardou.IntegrationTests;

public sealed class UsersTests : SignedInTests
{
    private const string NewPassword = "Third-user-pass1";

    [Fact]
    public async Task UserAdministrationRequiresASession()
    {
        HttpResponseMessage response = await Factory.CreateClient().GetAsync("/api/users", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UserAdministrationRequiresTheAdminRole()
    {
        HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage response = await bob.GetAsync("/api/users", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListReturnsAllUsersByName()
    {
        IReadOnlyList<UserAccount> users = await Client.ListUsersAsync();

        Assert.Equal([TestServerFactory.AdminUsername, BobUsername], users.Select(user => user.Username));
        Assert.Equal([true, false], users.Select(user => user.IsAdmin));
    }

    [Fact]
    public async Task CreateAddsAUserWithARootFolder()
    {
        HttpResponseMessage response = await Client.CreateUserAsync(new UserRequest("carol", "carol@example.com", NewPassword, IsAdmin: true));
        UserAccount carol = await UsersApi.ReadAccountAsync(response);
        HttpClient client = Factory.CreateClient();
        await client.LoginAsync("carol", NewPassword);

        CurrentUser? me = await client.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);
        HttpResponseMessage root = await client.GetAsync("/api/items/root", CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(new CurrentUser(carol.Id, "carol", "carol@example.com", IsAdmin: true), me);
        Assert.Equal(HttpStatusCode.OK, root.StatusCode);
    }

    [Fact]
    public async Task CreateRequiresAPassword()
    {
        HttpResponseMessage response = await Client.CreateUserAsync(new UserRequest("carol", "carol@example.com", Password: null, IsAdmin: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal([UserAccountErrorCodes.PasswordRequired], await UsersApi.ReadErrorCodesAsync(response));
    }

    [Fact]
    public async Task CreateRefusesATakenUsername()
    {
        HttpResponseMessage response = await Client.CreateUserAsync(new UserRequest(BobUsername, "other@example.com", NewPassword, IsAdmin: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["DuplicateUserName"], await UsersApi.ReadErrorCodesAsync(response));
        Assert.Equal(2, (await Client.ListUsersAsync()).Count);
    }

    [Fact]
    public async Task UpdateChangesAllFieldsAndKeepsTheFiles()
    {
        HttpClient bob = await SignInAsBobAsync();
        ItemEntry file = await bob.UploadEntryAsync(ItemsApi.Root, "notes.txt", "hello");
        UserAccount account = await Client.FindUserAsync(BobUsername);
        IReadOnlyList<string> blobs = BlobFiles();

        UserAccount updated = await UsersApi.ReadAccountAsync(
            await Client.UpdateUserAsync(account.Id, new UserRequest("robert", "robert@example.com", NewPassword, IsAdmin: true)));
        HttpResponseMessage oldSession = await bob.GetAsync("/api/auth/me", CancellationToken);
        HttpClient robert = Factory.CreateClient();
        await robert.LoginAsync("robert", NewPassword);
        ItemDetails details = await robert.GetItemAsync(file.Id);

        Assert.Equal(new UserAccount(account.Id, "robert", "robert@example.com", IsAdmin: true), updated);
        Assert.Equal(HttpStatusCode.Unauthorized, oldSession.StatusCode);
        Assert.Equal("notes.txt", details.Entry.Name);
        Assert.Equal(blobs, BlobFiles());
    }

    [Fact]
    public async Task UpdateKeepsThePasswordWhenNoneIsGiven()
    {
        UserAccount account = await Client.FindUserAsync(BobUsername);

        await Client.UpdateUserAsync(account.Id, new UserRequest(BobUsername, "bobby@example.com", Password: null, IsAdmin: false));
        HttpClient bob = await SignInAsBobAsync();
        CurrentUser? me = await bob.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);

        Assert.Equal("bobby@example.com", me?.Email);
    }

    [Fact]
    public async Task UpdateRefusesAPasswordThatBreaksThePolicy()
    {
        UserAccount account = await Client.FindUserAsync(BobUsername);

        HttpResponseMessage response = await Client.UpdateUserAsync(account.Id, new UserRequest("robert", account.Email, "short", IsAdmin: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("PasswordTooShort", await UsersApi.ReadErrorCodesAsync(response));
        Assert.Equal(BobUsername, (await Client.ListUsersAsync()).Single(user => user.Id == account.Id).Username);
    }

    [Fact]
    public async Task UpdateRefusesATakenEmail()
    {
        UserAccount account = await Client.FindUserAsync(BobUsername);

        HttpResponseMessage response = await Client.UpdateUserAsync(
            account.Id,
            new UserRequest(BobUsername, TestServerFactory.AdminEmail, Password: null, IsAdmin: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["DuplicateEmail"], await UsersApi.ReadErrorCodesAsync(response));
    }

    [Fact]
    public async Task UpdateRefusesToDemoteTheLastAdmin()
    {
        UserAccount admin = await Client.FindUserAsync(TestServerFactory.AdminUsername);

        HttpResponseMessage response = await Client.UpdateUserAsync(
            admin.Id,
            new UserRequest(admin.Username, admin.Email, Password: null, IsAdmin: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal([UserAccountErrorCodes.LastAdministrator], await UsersApi.ReadErrorCodesAsync(response));
    }

    [Fact]
    public async Task UpdateOfTheCurrentUserKeepsTheSession()
    {
        UserAccount admin = await Client.FindUserAsync(TestServerFactory.AdminUsername);

        HttpResponseMessage response = await Client.UpdateUserAsync(
            admin.Id,
            new UserRequest("root", "root@example.com", Password: null, IsAdmin: true));
        CurrentUser? me = await Client.GetFromJsonAsync<CurrentUser>("/api/auth/me", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new CurrentUser(admin.Id, "root", "root@example.com", IsAdmin: true), me);
    }

    [Fact]
    public async Task DemotionRemovesTheAdminAccessImmediately()
    {
        UserAccount account = await Client.FindUserAsync(BobUsername);
        await Client.UpdateUserAsync(account.Id, new UserRequest(BobUsername, account.Email, Password: null, IsAdmin: true));
        HttpClient bob = await SignInAsBobAsync();
        HttpResponseMessage asAdmin = await bob.GetAsync("/api/users", CancellationToken);

        await Client.UpdateUserAsync(account.Id, new UserRequest(BobUsername, account.Email, Password: null, IsAdmin: false));
        HttpResponseMessage afterDemotion = await bob.GetAsync("/api/users", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, asAdmin.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, afterDemotion.StatusCode);
    }

    [Fact]
    public async Task DeleteRemovesTheUserAndTheFiles()
    {
        HttpClient bob = await SignInAsBobAsync();
        ItemEntry folder = await bob.CreateFolderEntryAsync(ItemsApi.Root, "Photos");
        await bob.UploadEntryAsync(folder.Id, "photo.jpg", "image");
        await bob.UploadEntryAsync(ItemsApi.Root, "notes.txt", "hello");
        await Client.UploadEntryAsync(ItemsApi.Root, "admin.txt", "admin");
        UserAccount account = await Client.FindUserAsync(BobUsername);

        HttpResponseMessage response = await Client.DeleteUserAsync(account.Id);
        HttpResponseMessage login = await Factory.CreateClient().LoginAsync(BobUsername, BobPassword);
        HttpResponseMessage get = await Client.GetAsync($"/api/users/{account.Id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Single(BlobFiles());
    }

    [Fact]
    public async Task DeleteRefusesTheCurrentUser()
    {
        UserAccount admin = await Client.FindUserAsync(TestServerFactory.AdminUsername);

        HttpResponseMessage response = await Client.DeleteUserAsync(admin.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal([UserAccountErrorCodes.SelfDeletion], await UsersApi.ReadErrorCodesAsync(response));
    }

    [Fact]
    public async Task AnUnknownUserIsNotFound()
    {
        HttpResponseMessage response = await Client.DeleteUserAsync("missing");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
