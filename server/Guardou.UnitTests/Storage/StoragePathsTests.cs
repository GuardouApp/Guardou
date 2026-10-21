using Guardou.Infrastructure.Storage;

namespace Guardou.UnitTests.Storage;

public sealed class StoragePathsTests
{
    [Fact]
    public void KeepsAllDirectoriesInTheDataRoot()
    {
        string root = Path.GetFullPath(StoragePaths.DefaultRoot);
        StoragePaths paths = new(StoragePaths.DefaultRoot);

        Assert.Equal(Path.Combine(root, "files"), paths.FilesDirectory);
        Assert.Equal(Path.Combine(root, "db", "app.db"), paths.DatabaseFile);
        Assert.Equal(Path.Combine(root, "profile"), paths.ProfileDirectory);
        Assert.Equal(Path.Combine(root, "thumbs"), paths.ThumbsDirectory);
        Assert.Equal(Path.Combine(root, "cache", "tmp"), paths.TemporaryDirectory);
    }
}
