namespace Xperience.Relay.Docs;

static class RepoPaths
{
    /// <summary>Walks up from the current directory to the folder that holds the solution file.</summary>
    public static string FindRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Xperience.Relay.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Run from inside the repository, or pass --root <path>.");
    }
}
