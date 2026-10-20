using System.Text;
using Xperience.Relay.Contracts;
using Xperience.Relay.Docs;

// Keeps the generated parts of the docs site in step with the Contracts package.
//   dotnet run --project tools/Xperience.Relay.Docs            writes the generated files
//   dotnet run --project tools/Xperience.Relay.Docs -- --check  fails if any of them is out of date (used by CI)
//   --root <path>                                               the repository root (default: found from the current directory)

var check = args.Contains("--check");
var rootIndex = Array.IndexOf(args, "--root");
var root = rootIndex >= 0 && rootIndex + 1 < args.Length ? args[rootIndex + 1] : RepoPaths.FindRoot();

var contracts = typeof(RelayCommandAttribute).Assembly;
var docs = XmlDocs.Load(Path.ChangeExtension(contracts.Location, ".xml"));
var problems = new List<string>();
var catalog = Catalog.Build(contracts, docs, problems);
var outputs = new DocsGenerator(root, catalog).Generate(problems);

foreach (var problem in problems.Distinct())
{
    Console.Error.WriteLine("error: " + problem);
}

var stale = new List<string>();
foreach (var (path, content) in outputs.OrderBy(o => o.Key, StringComparer.Ordinal))
{
    var file = Path.Combine(root, path);
    var current = File.Exists(file) ? File.ReadAllText(file).Replace("\r\n", "\n") : null;

    if (current == content)
    {
        continue;
    }

    stale.Add(path);

    if (!check)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, content, new UTF8Encoding(false));
    }
}

if (check)
{
    foreach (var path in stale)
    {
        Console.Error.WriteLine($"stale: {path}");
    }

    if (stale.Count > 0)
    {
        Console.Error.WriteLine("The generated parts of the docs are out of date. Run: dotnet run --project tools/Xperience.Relay.Docs");
    }
}
else
{
    Console.WriteLine($"{outputs.Count} files checked, {stale.Count} written.");
}

return problems.Count > 0 || (check && stale.Count > 0) ? 1 : 0;
