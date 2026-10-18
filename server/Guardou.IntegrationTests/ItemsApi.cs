using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Guardou.Api.Models;
using Guardou.Application.Files;

namespace Guardou.IntegrationTests;

internal static class ItemsApi
{
    public const string Root = "root";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public static async Task<ItemDetails> GetItemAsync(this HttpClient client, string id, string query = "")
    {
        return await client.GetFromJsonAsync<ItemDetails>($"/api/items/{id}{query}", HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The item is missing.");
    }

    public static Task<HttpResponseMessage> CreateFolderAsync(this HttpClient client, string parentId, string name)
    {
        return client.PostAsJsonAsync($"/api/items/{parentId}/folders", new NameRequest(name), CancellationToken);
    }

    public static async Task<ItemEntry> CreateFolderEntryAsync(this HttpClient client, string parentId, string name)
    {
        return await ReadEntryAsync(await client.CreateFolderAsync(parentId, name));
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
        MultipartFormDataContent form = new();
        ByteArrayContent file = new(Encoding.UTF8.GetBytes(content));
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(file, "file", fileName);
        form.Add(new StringContent(conflict.ToString()), "conflict");
        if (relativePath is not null)
        {
            form.Add(new StringContent(relativePath), "relativePath");
        }

        if (modified is { } date)
        {
            form.Add(new StringContent(date.ToString("O", CultureInfo.InvariantCulture)), "modified");
        }

        return client.PostAsync($"/api/items/{folderId}/files", form, CancellationToken);
    }

    public static async Task<ItemEntry> UploadEntryAsync(this HttpClient client, string folderId, string fileName, string content)
    {
        return await ReadEntryAsync(await client.UploadAsync(folderId, fileName, content));
    }

    public static Task<HttpResponseMessage> TransferAsync(
        this HttpClient client,
        string operation,
        string id,
        string destinationId,
        ConflictPolicy conflict = ConflictPolicy.Fail)
    {
        return client.PostAsJsonAsync(
            $"/api/items/{id}/{operation}",
            new TransferRequest(destinationId, conflict),
            HttpClientExtensions.JsonOptions,
            CancellationToken);
    }

    public static Task<HttpResponseMessage> PostSelectionAsync(this HttpClient client, string path, params string[] ids)
    {
        return client.PostAsJsonAsync(path, new ItemSelection(ids), CancellationToken);
    }

    public static async Task<ItemEntry> ReadEntryAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ItemEntry>(HttpClientExtensions.JsonOptions, CancellationToken)
            ?? throw new InvalidOperationException("The entry is missing.");
    }

    public static IEnumerable<string> Names(this ItemDetails details)
    {
        return details.Items?.Select(item => item.Name) ?? [];
    }
}
