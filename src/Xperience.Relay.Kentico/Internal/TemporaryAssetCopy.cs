namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// A throwaway copy of an existing asset file. When nothing published still uses an asset, Kentico deletes its old file
/// before it reads the replacement, so re-submitting the original path reads a file that was just removed and the asset
/// is lost. Re-submitting a copy leaves Kentico free to delete the original.
/// </summary>
internal sealed class TemporaryAssetCopy : IDisposable
{
    private TemporaryAssetCopy(string filePath) => FilePath = filePath;

    public string FilePath { get; }

    /// <summary>Copies the asset's file (found through Kentico's storage), or returns null if it isn't there.</summary>
    public static TemporaryAssetCopy? TryCreate(string assetFilePath) =>
        TryCreate(assetFilePath, CMS.IO.File.Exists, CMS.IO.File.ReadAllBytes);

    public static TemporaryAssetCopy? TryCreate(string assetFilePath, Func<string, bool> exists, Func<string, byte[]> readAllBytes)
    {
        if (!exists(assetFilePath))
        {
            return null;
        }

        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + Path.GetExtension(assetFilePath));
        File.WriteAllBytes(filePath, readAllBytes(assetFilePath));

        return new TemporaryAssetCopy(filePath);
    }

    public void Dispose()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}
