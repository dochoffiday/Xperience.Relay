using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class TemporaryAssetCopyTests
{
    private const string AssetPath = "assets/contentitems/ab/abc/field/31cb66f0.jpg";
    private static readonly byte[] Bytes = [1, 2, 3, 4];

    [Fact]
    public void CopiesTheAssetFileAndKeepsItsExtension()
    {
        using var copy = TemporaryAssetCopy.TryCreate(AssetPath, _ => true, _ => Bytes);

        Assert.NotNull(copy);
        Assert.EndsWith(".jpg", copy.FilePath);
        Assert.Equal(Bytes, File.ReadAllBytes(copy.FilePath));
    }

    [Fact]
    public void ReadsTheOriginalFromTheGivenPath()
    {
        string? readFrom = null;

        using var copy = TemporaryAssetCopy.TryCreate(AssetPath, _ => true, path =>
        {
            readFrom = path;
            return Bytes;
        });

        Assert.Equal(AssetPath, readFrom);
    }

    [Fact]
    public void WhenTheAssetFileIsMissing_ReturnsNullWithoutReading()
    {
        var read = false;

        var copy = TemporaryAssetCopy.TryCreate(AssetPath, _ => false, _ =>
        {
            read = true;
            return Bytes;
        });

        Assert.Null(copy);
        Assert.False(read);
    }

    [Fact]
    public void DisposingDeletesTheCopy_AndDoingItTwiceIsFine()
    {
        var copy = TemporaryAssetCopy.TryCreate(AssetPath, _ => true, _ => Bytes)!;
        Assert.True(File.Exists(copy.FilePath));

        copy.Dispose();
        copy.Dispose();

        Assert.False(File.Exists(copy.FilePath));
    }
}
