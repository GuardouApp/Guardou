using System.Net;
using System.Net.Http.Json;
using System.Text;
using Guardou.Application.Files;
using Guardou.Application.Uploads;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed class UploadsTests : SignedInTests
{
    private static readonly byte[] Content = Encoding.UTF8.GetBytes("0123456789");

    [Fact]
    public async Task ResumesAnUploadFromTheStoredOffset()
    {
        UploadSession session = await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);

        HttpResponseMessage first = await Client.WriteUploadAsync(session.Id, 0, Content[..4]);
        HttpResponseMessage info = await Client.GetUploadInfoAsync(session.Id);
        HttpResponseMessage second = await Client.WriteUploadAsync(session.Id, UploadsApi.OffsetOf(info), Content[4..]);
        ItemEntry file = await ItemsApi.ReadEntryAsync(await Client.CompleteUploadAsync(session.Id));

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(4, UploadsApi.OffsetOf(info));
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        Assert.Equal(Content.Length, file.Size);
        Assert.Equal(Content, await Client.GetByteArrayAsync($"/api/items/{file.Id}/download", CancellationToken));
    }

    [Fact]
    public async Task RefusesAWriteAtTheWrongOffset()
    {
        UploadSession session = await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);
        await Client.WriteUploadAsync(session.Id, 0, Content[..4]);

        HttpResponseMessage response = await Client.WriteUploadAsync(session.Id, 2, Content[2..]);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RefusesToCompleteAnIncompleteUpload()
    {
        UploadSession session = await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);
        await Client.WriteUploadAsync(session.Id, 0, Content[..4]);

        HttpResponseMessage response = await Client.CompleteUploadAsync(session.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefusesASecondUploadToTheSameDestination()
    {
        await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);

        HttpResponseMessage second = await Client.StartUploadAsync(ItemsApi.Root, "report.txt", Content.Length, ConflictPolicy.Replace);
        HttpResponseMessage other = await Client.StartUploadAsync(ItemsApi.Root, "other.txt", Content.Length);
        ProblemDetails? problem = await second.Content.ReadFromJsonAsync<ProblemDetails>(CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal($"[\"{UploadInProgressException.Code}\"]", problem?.Extensions["codes"]?.ToString());
        Assert.Equal(HttpStatusCode.Created, other.StatusCode);
    }

    [Fact]
    public async Task RefusesASecondUploadToTheSameFolderPath()
    {
        await Client.StartUploadAsync(ItemsApi.Root, "a.txt", Content.Length, relativePath: "photos/a.txt");

        HttpResponseMessage same = await Client.StartUploadAsync(ItemsApi.Root, "a.txt", Content.Length, relativePath: "photos/a.txt");
        HttpResponseMessage otherFolder = await Client.StartUploadAsync(ItemsApi.Root, "a.txt", Content.Length, relativePath: "music/a.txt");

        Assert.Equal(HttpStatusCode.Conflict, same.StatusCode);
        Assert.Equal(HttpStatusCode.Created, otherFolder.StatusCode);
    }

    [Fact]
    public async Task PermitsANewUploadAfterTheActiveUploadIsIdle()
    {
        await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);
        Factory.Time.Advance(TimeSpan.FromMinutes(31));

        HttpResponseMessage response = await Client.StartUploadAsync(ItemsApi.Root, "report.txt", Content.Length);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PermitsANewUploadAfterACancel()
    {
        UploadSession session = await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);

        HttpResponseMessage cancelled = await Client.CancelUploadAsync(session.Id);
        HttpResponseMessage info = await Client.GetUploadInfoAsync(session.Id);
        HttpResponseMessage started = await Client.StartUploadAsync(ItemsApi.Root, "report.txt", Content.Length);

        Assert.Equal(HttpStatusCode.NoContent, cancelled.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, info.StatusCode);
        Assert.Equal(HttpStatusCode.Created, started.StatusCode);
    }

    [Fact]
    public async Task ChecksTheNameWhenAnUploadStarts()
    {
        await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "old");

        HttpResponseMessage failed = await Client.StartUploadAsync(ItemsApi.Root, "report.txt", Content.Length);
        HttpResponseMessage skipped = await Client.StartUploadAsync(ItemsApi.Root, "report.txt", Content.Length, ConflictPolicy.Skip);
        HttpResponseMessage renamed = await Client.StartUploadAsync(ItemsApi.Root, "report.txt", Content.Length, ConflictPolicy.Rename);

        Assert.Equal(HttpStatusCode.Conflict, failed.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, skipped.StatusCode);
        Assert.Equal(HttpStatusCode.Created, renamed.StatusCode);
    }

    [Fact]
    public async Task AppliesTheConflictPolicyWhenAnUploadCompletes()
    {
        UploadSession session = await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);
        await Client.WriteUploadAsync(session.Id, 0, Content);
        Factory.Time.Advance(TimeSpan.FromMinutes(31));
        await Client.UploadEntryAsync(ItemsApi.Root, "report.txt", "other");

        HttpResponseMessage failed = await Client.CompleteUploadAsync(session.Id);
        ItemEntry renamed = await ItemsApi.ReadEntryAsync(await Client.CompleteUploadAsync(session.Id, ConflictPolicy.Rename));

        Assert.Equal(HttpStatusCode.Conflict, failed.StatusCode);
        Assert.Equal("report (1).txt", renamed.Name);
    }

    [Fact]
    public async Task HidesTheUploadsOfOtherUsers()
    {
        UploadSession session = await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);
        using HttpClient bob = await SignInAsBobAsync();

        HttpResponseMessage info = await bob.GetUploadInfoAsync(session.Id);
        HttpResponseMessage write = await bob.WriteUploadAsync(session.Id, 0, Content);
        HttpResponseMessage complete = await bob.CompleteUploadAsync(session.Id);
        HttpResponseMessage cancel = await bob.CancelUploadAsync(session.Id);

        Assert.Equal(HttpStatusCode.NotFound, info.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, write.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, complete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, cancel.StatusCode);
    }

    [Fact]
    public async Task RefusesAWriteWithoutTheAntiforgeryToken()
    {
        UploadSession session = await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);
        Client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");

        HttpResponseMessage response = await Client.WriteUploadAsync(session.Id, 0, Content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RemovesExpiredUploads()
    {
        UploadSession session = await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);
        Factory.Time.Advance(TimeSpan.FromHours(25));

        await using (AsyncServiceScope scope = Factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<UploadCleanup>().RemoveExpiredAsync(CancellationToken);
        }

        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetUploadInfoAsync(session.Id)).StatusCode);
    }

    [Fact]
    public async Task RefusesAnAnonymousWrite()
    {
        UploadSession session = await Client.StartUploadSessionAsync(ItemsApi.Root, "report.txt", Content.Length);
        using HttpClient anonymous = Factory.CreateClient();

        HttpResponseMessage response = await anonymous.WriteUploadAsync(session.Id, 0, Content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
