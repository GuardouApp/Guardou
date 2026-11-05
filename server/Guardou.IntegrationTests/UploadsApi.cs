using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Guardou.Api.Models;
using Guardou.Application.Files;
using Guardou.Application.Uploads;

namespace Guardou.IntegrationTests;

internal static class UploadsApi
{
    public const string UploadsPath = "/api/uploads";

    private const string TusResumable = "Tus-Resumable";
    private const string TusVersion = "1.0.0";
    private const string UploadOffset = "Upload-Offset";
    private const string OffsetContentType = "application/offset+octet-stream";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public static Task<HttpResponseMessage> StartUploadAsync(
        this HttpClient client,
        string folderId,
        string fileName,
        long size,
        ConflictPolicy conflict = ConflictPolicy.Fail,
        string? relativePath = null,
        DateTimeOffset? modified = null)
    {
        return client.PostAsJsonAsync(
            UploadsPath,
            new StartUploadRequest(folderId, fileName, relativePath, size, modified, conflict),
            HttpClientExtensions.JsonOptions,
            CancellationToken);
    }

    public static async Task<UploadSession> StartUploadSessionAsync(this HttpClient client, string folderId, string fileName, long size)
    {
        HttpResponseMessage response = await client.StartUploadAsync(folderId, fileName, size);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UploadSession>(HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The upload session is missing.");
    }

    public static Task<HttpResponseMessage> WriteUploadAsync(this HttpClient client, string id, long offset, byte[] content)
    {
        ByteArrayContent body = new(content);
        body.Headers.ContentType = new MediaTypeHeaderValue(OffsetContentType);
        HttpRequestMessage request = new(HttpMethod.Patch, ContentPath(id)) { Content = body };
        request.Headers.Add(TusResumable, TusVersion);
        request.Headers.Add(UploadOffset, offset.ToString(CultureInfo.InvariantCulture));
        return client.SendAsync(request, CancellationToken);
    }

    public static Task<HttpResponseMessage> GetUploadInfoAsync(this HttpClient client, string id)
    {
        HttpRequestMessage request = new(HttpMethod.Head, ContentPath(id));
        request.Headers.Add(TusResumable, TusVersion);
        return client.SendAsync(request, CancellationToken);
    }

    public static long OffsetOf(HttpResponseMessage response)
    {
        return long.Parse(response.Headers.GetValues(UploadOffset).Single(), CultureInfo.InvariantCulture);
    }

    public static Task<HttpResponseMessage> CompleteUploadAsync(this HttpClient client, string id, ConflictPolicy conflict = ConflictPolicy.Fail)
    {
        return client.PostAsJsonAsync(
            $"{UploadsPath}/{id}/complete",
            new CompleteUploadRequest(conflict),
            HttpClientExtensions.JsonOptions,
            CancellationToken);
    }

    public static Task<HttpResponseMessage> CancelUploadAsync(this HttpClient client, string id)
    {
        return client.DeleteAsync($"{UploadsPath}/{id}", CancellationToken);
    }

    public static Task<HttpResponseMessage> UploadAsync(
        this HttpClient client,
        string folderId,
        string fileName,
        string content,
        ConflictPolicy conflict = ConflictPolicy.Fail,
        string? relativePath = null,
        DateTimeOffset? modified = null)
    {
        return client.UploadBytesAsync(folderId, fileName, Encoding.UTF8.GetBytes(content), conflict, relativePath, modified);
    }

    public static async Task<HttpResponseMessage> UploadBytesAsync(
        this HttpClient client,
        string folderId,
        string fileName,
        byte[] content,
        ConflictPolicy conflict = ConflictPolicy.Fail,
        string? relativePath = null,
        DateTimeOffset? modified = null)
    {
        HttpResponseMessage started = await client.StartUploadAsync(folderId, fileName, content.Length, conflict, relativePath, modified);
        if (started.StatusCode != HttpStatusCode.Created)
        {
            return started;
        }

        UploadSession session = await started.Content.ReadFromJsonAsync<UploadSession>(HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The upload session is missing.");
        (await client.WriteUploadAsync(session.Id, 0, content)).EnsureSuccessStatusCode();
        return await client.CompleteUploadAsync(session.Id, conflict);
    }

    public static async Task<ItemEntry> UploadEntryAsync(this HttpClient client, string folderId, string fileName, string content)
    {
        return await ItemsApi.ReadEntryAsync(await client.UploadAsync(folderId, fileName, content));
    }

    private static string ContentPath(string id)
    {
        return $"{UploadsPath}/content/{id}";
    }
}
