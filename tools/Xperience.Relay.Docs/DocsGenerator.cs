using System.Text;
using System.Text.RegularExpressions;

namespace Xperience.Relay.Docs;

/// <summary>
/// Works out what every generated file should contain. Command pages hold hand-written prose around blocks the generator owns,
/// marked <c>&lt;!-- relay:begin name --&gt;</c> ... <c>&lt;!-- relay:end name --&gt;</c>; everything outside the markers is left alone.
/// </summary>
sealed partial class DocsGenerator(string root, Catalog catalog)
{
    private const string NavBegin = "# relay:begin nav-commands";
    private const string NavEnd = "# relay:end nav-commands";

    private readonly string _docs = Path.Combine(root, "docs");
    private readonly Dictionary<string, string> _outputs = new(StringComparer.Ordinal);

    /// <summary>The new content of every file the generator manages, by path relative to the repository root.</summary>
    public IReadOnlyDictionary<string, string> Generate(List<string> problems)
    {
        CheckDocumented(problems);

        foreach (var command in catalog.Commands)
        {
            GenerateCommandPage(command, problems);
        }

        foreach (var area in Areas.All)
        {
            GenerateAreaPage(area, problems);
        }

        _outputs["docs/commands/index.md"] = Normalize(Pages.CommandIndex(catalog));
        _outputs["docs/reference/types.md"] = Normalize(Pages.TypesPage(catalog));

        GenerateSharedBlocks(problems);
        GenerateNavigation(problems);
        CheckForStrayPages(problems);

        foreach (var (path, content) in _outputs)
        {
            if (content.Contains("TODO", StringComparison.Ordinal))
            {
                problems.Add($"{path} still has a TODO in it.");
            }
        }

        return _outputs;
    }

    /// <summary>Every parameter and every field of a result is described in the XML docs, since the tables are made from them.</summary>
    private void CheckDocumented(List<string> problems)
    {
        foreach (var model in catalog.Commands.Cast<TypeModel>().Concat(catalog.Types))
        {
            if (model.Summary is null)
            {
                problems.Add($"{model.Name} has no XML summary.");
            }

            foreach (var property in model.Properties.Where(p => p.Summary is null))
            {
                problems.Add($"{model.Name}.{property.Name} has no XML summary.");
            }
        }
    }

    private void GenerateCommandPage(CommandModel command, List<string> problems)
    {
        var path = "docs/" + command.PagePath;
        var links = new Links(catalog, command.PagePath);
        var examples = ExampleStore.Load(Path.Combine(_docs, "examples"), command.Verb, problems);

        var page = ReadOrScaffold(path, () => CommandScaffold(command));

        page = ApplyBlock(page, "header", Pages.Header(command, catalog, links), path, problems);
        page = ApplyBlock(page, "parameters", Pages.Parameters(command, catalog, links), path, problems);
        page = ApplyBlock(page, "result", Pages.Result(command, catalog, links), path, problems);
        page = ApplyBlock(page, "examples", Pages.Examples(command, examples, links, problems), path, problems);
        page = ApplyBlock(page, "errors", Pages.Errors(command, examples), path, problems);

        _outputs[path] = Normalize(page);
    }

    private void GenerateAreaPage(Area area, List<string> problems)
    {
        var path = $"docs/commands/{area.Folder}/index.md";
        var links = new Links(catalog, $"commands/{area.Folder}/index.md");

        var page = ReadOrScaffold(path, () => AreaScaffold(area));
        page = ApplyBlock(page, "commands", Pages.AreaCommands(area, catalog, links), path, problems);

        _outputs[path] = Normalize(page);
    }

    /// <summary>Blocks any page may include, such as the commands of each access tier.</summary>
    private void GenerateSharedBlocks(List<string> problems)
    {
        if (!Directory.Exists(_docs))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(_docs, "*.md", SearchOption.AllDirectories))
        {
            var path = Relative(file);
            if (_outputs.ContainsKey(path))
            {
                continue;
            }

            var page = File.ReadAllText(file);
            if (!page.Contains("<!-- relay:begin tiers -->", StringComparison.Ordinal))
            {
                continue;
            }

            var links = new Links(catalog, path["docs/".Length..]);
            _outputs[path] = Normalize(ApplyBlock(page, "tiers", Pages.Tiers(catalog, links), path, problems));
        }
    }

    private void GenerateNavigation(List<string> problems)
    {
        const string path = "mkdocs.yml";
        var file = Path.Combine(root, path);
        if (!File.Exists(file))
        {
            return;
        }

        var text = Normalize(File.ReadAllText(file));
        var begin = text.IndexOf(NavBegin, StringComparison.Ordinal);
        var end = text.IndexOf(NavEnd, StringComparison.Ordinal);

        if (begin < 0 || end < begin)
        {
            problems.Add($"{path} needs the lines '{NavBegin}' and '{NavEnd}' around the commands of the navigation.");
            return;
        }

        var indent = new string(' ', begin - (text.LastIndexOf('\n', begin) + 1));
        var lineEnd = text.IndexOf('\n', begin) + 1;
        var endLineStart = text.LastIndexOf('\n', end) + 1;

        _outputs[path] = Normalize(text[..lineEnd] + Pages.NavCommands(catalog, indent) + "\n" + text[endLineStart..]);
    }

    private void CheckForStrayPages(List<string> problems)
    {
        var commands = Path.Combine(_docs, "commands");
        if (!Directory.Exists(commands))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(commands, "*.md", SearchOption.AllDirectories))
        {
            var path = Relative(file);
            if (!_outputs.ContainsKey(path))
            {
                problems.Add($"{path} isn't the page of any command (a command was renamed or removed?).");
            }
        }
    }

    private string ReadOrScaffold(string path, Func<string> scaffold)
    {
        var file = Path.Combine(root, path);
        return File.Exists(file) ? File.ReadAllText(file) : scaffold();
    }

    private string Relative(string file) => Path.GetRelativePath(root, file).Replace('\\', '/');

    private static string ApplyBlock(string page, string name, string content, string path, List<string> problems)
    {
        var begin = $"<!-- relay:begin {name} -->";
        var end = $"<!-- relay:end {name} -->";

        page = page.Replace("\r\n", "\n");
        var start = page.IndexOf(begin, StringComparison.Ordinal);
        var stop = page.IndexOf(end, StringComparison.Ordinal);

        if (start < 0 || stop < start)
        {
            problems.Add($"{path} needs the lines '{begin}' and '{end}'.");
            return page;
        }

        var inner = content.Length == 0 ? "\n" : "\n" + content.TrimEnd() + "\n";
        return page[..(start + begin.Length)] + inner + page[stop..];
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd() + "\n";

    private static string CommandScaffold(CommandModel command) => $"""
        # {command.Verb}

        <!-- relay:begin header -->
        <!-- relay:end header -->

        TODO: say what the command does.

        <!-- relay:begin parameters -->
        <!-- relay:end parameters -->

        <!-- relay:begin result -->
        <!-- relay:end result -->

        ## Behavior

        TODO: how the command behaves.

        <!-- relay:begin examples -->
        <!-- relay:end examples -->

        <!-- relay:begin errors -->
        <!-- relay:end errors -->

        ## See also

        TODO: related commands.
        """;

    private static string AreaScaffold(Area area) => $"""
        # {area.Title}

        TODO: introduce this group of commands.

        <!-- relay:begin commands -->
        <!-- relay:end commands -->
        """;
}
