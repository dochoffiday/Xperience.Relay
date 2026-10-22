using System.Text;
using System.Text.RegularExpressions;

namespace Xperience.Relay.Docs;

/// <summary>
/// Works out what every generated file should contain. The page of each area has a section for every one of its commands,
/// with hand-written prose around blocks the generator owns, marked <c>&lt;!-- relay:begin name --&gt;</c> ...
/// <c>&lt;!-- relay:end name --&gt;</c> (a command's blocks are named <c>header:verb</c>, <c>parameters:verb</c> and so on);
/// everything outside the markers is left alone.
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

    private void GenerateAreaPage(Area area, List<string> problems)
    {
        var pagePath = $"commands/{area.Folder}/index.md";
        var path = "docs/" + pagePath;
        var links = new Links(catalog, pagePath);

        var page = ReadOrScaffold(path, () => AreaScaffold(area)).Replace("\r\n", "\n");
        page = ApplyBlock(page, "commands", Pages.AreaCommands(area, catalog, links), path, problems);

        foreach (var verb in area.Verbs)
        {
            var command = catalog.Commands.First(c => c.Verb == verb);
            var examples = ExampleStore.Load(Path.Combine(_docs, "examples"), verb, problems);

            // A command with no section yet gets an empty one, with TODOs to fill in, at the end of the page.
            if (!page.Contains($"<!-- relay:begin header:{verb} -->", StringComparison.Ordinal))
            {
                page = page.TrimEnd() + "\n\n" + CommandScaffold(verb);
            }

            page = ApplyBlock(page, $"header:{verb}", Pages.Header(command, catalog, links), path, problems);
            page = ApplyBlock(page, $"parameters:{verb}", Pages.Parameters(command, catalog, links), path, problems);
            page = ApplyBlock(page, $"result:{verb}", Pages.Result(command, catalog, links), path, problems);
            page = ApplyBlock(page, $"examples:{verb}", Pages.Examples(command, examples, links, problems), path, problems);
            page = ApplyBlock(page, $"errors:{verb}", Pages.Errors(command, examples), path, problems);

            // The menu and the other pages link to this heading, and MkDocs can't check the links of the navigation.
            if (!Regex.IsMatch(page, $"^## {Regex.Escape(verb)}\\s*$", RegexOptions.Multiline))
            {
                problems.Add($"{path} needs the heading '## {verb}' for the command; the menu and other pages link to it.");
            }
        }

        foreach (Match section in SectionMarker().Matches(page))
        {
            if (!area.Verbs.Contains(section.Groups["verb"].Value))
            {
                problems.Add($"{path} has a section for '{section.Groups["verb"].Value}', which isn't a command of {area.Title} (renamed or removed?).");
            }
        }

        _outputs[path] = Normalize(page);
    }

    [GeneratedRegex(@"<!-- relay:begin header:(?<verb>[a-z-]+) -->")]
    private static partial Regex SectionMarker();

    /// <summary>
    /// A command in the navigation is a link to a section of its area's page, which MkDocs can't check, so the files the
    /// navigation does name are checked here instead.
    /// </summary>
    private void CheckNavigationFiles(string mkdocs, List<string> problems)
    {
        foreach (Match entry in NavigationFile().Matches(mkdocs))
        {
            var target = entry.Groups["path"].Value;
            if (!File.Exists(Path.Combine(_docs, target)))
            {
                problems.Add($"mkdocs.yml: the navigation names docs/{target}, which doesn't exist.");
            }
        }
    }

    [GeneratedRegex(@"^\s*-\s+(?:[^:#\n]+:\s+)?(?<path>[A-Za-z0-9_./-]+\.md)\s*$", RegexOptions.Multiline)]
    private static partial Regex NavigationFile();

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

        var updated = Normalize(text[..lineEnd] + Pages.NavCommands(catalog, indent) + "\n" + text[endLineStart..]);
        _outputs[path] = updated;

        CheckNavigationFiles(updated, problems);
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

    /// <summary>
    /// The section of a command on its area's page. Its heading is the verb, which is what the navigation links to; the
    /// headings inside it carry the verb in their ids, since every command has a "Parameters" and ids are unique on a page.
    /// </summary>
    private static string CommandScaffold(string verb) => $$"""
        ---

        ## {{verb}}

        <!-- relay:begin header:{{verb}} -->
        <!-- relay:end header:{{verb}} -->

        TODO: say what the command does.

        <!-- relay:begin parameters:{{verb}} -->
        <!-- relay:end parameters:{{verb}} -->

        <!-- relay:begin result:{{verb}} -->
        <!-- relay:end result:{{verb}} -->

        ### Behavior { #{{verb}}-behavior }

        TODO: how the command behaves.

        <!-- relay:begin examples:{{verb}} -->
        <!-- relay:end examples:{{verb}} -->

        <!-- relay:begin errors:{{verb}} -->
        <!-- relay:end errors:{{verb}} -->

        ### See also { #{{verb}}-see-also }

        TODO: related commands.
        """;

    /// <summary>
    /// The page of an area. <c>toc_depth: 2</c> keeps the table of contents to the commands: the sections inside a command
    /// would add six entries each (see <c>hooks/toc_depth.py</c>).
    /// </summary>
    private static string AreaScaffold(Area area) => $"""
        ---
        toc_depth: 2
        ---

        # {area.Title}

        TODO: introduce this group of commands.

        <!-- relay:begin commands -->
        <!-- relay:end commands -->
        """;
}
