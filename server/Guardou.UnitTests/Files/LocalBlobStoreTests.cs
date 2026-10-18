using System.Text;
using Guardou.Infrastructure.Files;
using Guardou.Infrastructure.Storage;

namespace Guardou.UnitTests.Files;

public sealed class LocalBlobStoreTests : IDisposable
{
    private static readonly DateTimeOffset Modified = new(2024, 5, 6, 7, 8, 9, TimeSpan.Zero);

    private readonly TemporaryDirectory _data = new();
    private readonly LocalBlobStore _store;

    public LocalBlobStoreTests()
    {
        _store = new LocalBlobStore(new StoragePaths(_data.Combine("data"), _data.Combine("db")));
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _data.Dispose();
    }

    [Fact]
    public async Task WritesBlobsWithTheModifiedDate()
    {
        long size = await WriteAsync("owner/2024/2024-05-06/a.txt", "hello");

        string path = _data.Combine("data", "owner", "2024", "2024-05-06", "a.txt");
        Assert.Equal(5, size);
        Assert.Equal("hello", await File.ReadAllTextAsync(path, CancellationToken));
        Assert.Equal(Modified.UtcDateTime, File.GetLastWriteTimeUtc(path));
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(path) ?? _data.Path, "*.part"));
    }

    [Fact]
    public async Task CopiesAndDeletesBlobs()
    {
        await WriteAsync("owner/a.txt", "hello");

        await _store.CopyAsync("owner/a.txt", "owner/b.txt", CancellationToken);
        _store.Delete("owner/a.txt");

        Assert.False(File.Exists(_data.Combine("data", "owner", "a.txt")));
        await using Stream copy = _store.OpenRead("owner/b.txt");
        using StreamReader reader = new(copy);
        Assert.Equal("hello", await reader.ReadToEndAsync(CancellationToken));
    }

    [Fact]
    public async Task DoesNotOverwriteBlobs()
    {
        await WriteAsync("owner/a.txt", "hello");

        await Assert.ThrowsAsync<IOException>(() => WriteAsync("owner/a.txt", "other"));
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("owner/../../outside.txt")]
    [InlineData("")]
    public async Task RefusesKeysOutsideTheFilesDirectory(string key)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => WriteAsync(key, "x"));
        Assert.Throws<InvalidOperationException>(() => _store.Delete(key));
    }

    [Fact]
    public void ReportsTheDiskSize()
    {
        Assert.True(_store.TotalBytes() > 0);
    }

    private async Task<long> WriteAsync(string key, string content)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(content));
        return await _store.WriteAsync(key, stream, Modified, CancellationToken);
    }
}
