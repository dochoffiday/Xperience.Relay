using System.Text;
using System.Text.RegularExpressions;
using Xperience.Relay.Contracts;

namespace Xperience.Relay.Docs;

/// <summary>Writes the generated parts of the pages.</summary>
static partial class Pages
{
    public static string Description(System.Xml.Linq.XElement? summary, Links links) => XmlText.ToMarkdown(summary, links);

    public static string FirstSentence(CommandModel command, Links links) => XmlText.FirstSentence(Description(command.Summary, links));

    private static string AccessLink(RelayAccess access, Links links) => $"[{access}]({links.To(Links.AccessTiersPage)})";

    // ---------------------------------------------------------------------------------------------------------------
    // Command pages

    public static string Header(CommandModel command, Catalog catalog, Links links)
    {
        var returns = command.ResultType is { } type && catalog.TypeFor(type) is { } model
            ? $"[`{model.Name}`]({links.ToType(model)})"
            : "nothing; the `message` says what happened";

        return $"""
            | Verb | Access | Returns | C# command |
            |---|---|---|---|
            | `{command.Verb}` | {AccessLink(command.Access, links)} | {returns} | `{command.Type.Name}` |
            """;
    }

    public static string Parameters(CommandModel command, Catalog catalog, Links links)
    {
        var text = new StringBuilder("## Parameters\n\n");

        if (command.Properties.Count == 0)
        {
            return text.Append("This command takes no parameters. Send an empty `parameters` object, or leave `parameters` out.").ToString();
        }

        text.AppendLine("Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.").AppendLine();
        text.AppendLine("| Parameter | Type | Required | Description |").AppendLine("|---|---|---|---|");

        foreach (var property in command.Properties)
        {
            var description = Description(property.Summary, links);
            text.AppendLine($"| `{property.JsonName}` | {TypeText.Of(property.Type, catalog, links)} | {(IsRequired(description) ? "Yes" : "No")} | {WithoutRequiredMarker(description)} |");
        }

        return text.ToString().TrimEnd();
    }

    public static string Result(CommandModel command, Catalog catalog, Links links)
    {
        var text = new StringBuilder("## Result\n\n");

        if (command.ResultType is not { } type || catalog.TypeFor(type) is not { } model)
        {
            return text.Append("A successful run returns no data (`data` is `null`); the `message` says what was done.").ToString();
        }

        text.AppendLine($"A successful run returns a [{model.Name}]({links.ToType(model)}) in `data`:").AppendLine();
        text.Append(PropertyTable(model, catalog, links, "Field"));
        return text.ToString().TrimEnd();
    }

    public static string Examples(CommandModel command, IReadOnlyList<Example> examples, Links links, List<string> problems)
    {
        var successes = examples.Where(e => !e.IsFailure).ToList();
        if (successes.Count == 0)
        {
            return string.Empty;
        }

        var text = new StringBuilder("## Examples\n\n");
        text.AppendLine("The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.").AppendLine();

        foreach (var example in successes)
        {
            var source = $"docs/examples/{command.Verb}/{example.Name}.json";
            var csharp = CSharpSnippet.For(command, example.Parameters, problems, source);

            text.AppendLine($"### {example.Title}").AppendLine();

            if (example.Note is { Length: > 0 } note)
            {
                text.AppendLine(note).AppendLine();
            }

            text.AppendLine("=== \"curl\"").AppendLine();
            text.AppendLine(Indent(Fence("bash", Curl(command.Verb, example.Parameters)))).AppendLine();
            text.AppendLine("=== \"C#\"").AppendLine();
            text.AppendLine(Indent(Fence("csharp", csharp))).AppendLine();

            text.AppendLine($"Response (HTTP {example.Status}):").AppendLine();
            text.AppendLine(Fence("json", ExampleStore.Indented(example.Response))).AppendLine();
        }

        return text.ToString().TrimEnd();
    }

    public static string Errors(CommandModel command, IReadOnlyList<Example> examples)
    {
        var failures = examples.Where(e => e.IsFailure).ToList();
        if (failures.Count == 0)
        {
            return string.Empty;
        }

        var text = new StringBuilder("## Errors\n\n");
        text.AppendLine("A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:").AppendLine();

        foreach (var failure in failures)
        {
            text.AppendLine($"???+ failure \"{failure.Title.Replace("\"", "'")}\"").AppendLine();

            if (failure.Note is { Length: > 0 } note)
            {
                text.AppendLine(Indent(note)).AppendLine();
            }

            text.AppendLine(Indent("Parameters:")).AppendLine();
            text.AppendLine(Indent(Fence("json", ExampleStore.Indented(failure.Parameters)))).AppendLine();
            text.AppendLine(Indent("Response:")).AppendLine();
            text.AppendLine(Indent(Fence("json", ExampleStore.Indented(failure.Response)))).AppendLine();
        }

        return text.ToString().TrimEnd();
    }

    // ---------------------------------------------------------------------------------------------------------------
    // Landing pages and reference pages

    public static string AreaCommands(Area area, Catalog catalog, Links links)
    {
        var text = new StringBuilder("| Command | Access | What it does |\n|---|---|---|\n");

        foreach (var verb in area.Verbs)
        {
            var command = catalog.Commands.First(c => c.Verb == verb);
            text.AppendLine($"| {links.Command(command)} | {command.Access} | {FirstSentence(command, links)} |");
        }

        return text.ToString().TrimEnd();
    }

    public static string CommandIndex(Catalog catalog)
    {
        var links = new Links(catalog, "commands/index.md");
        var text = new StringBuilder();

        text.AppendLine("# Commands").AppendLine();
        text.AppendLine($"Relay has {catalog.Commands.Count} commands, each with its own page. A command is identified by its **verb**, " +
                        "takes its parameters as JSON, and answers with a [result](../reference/types.md#relaycommandresult). " +
                        $"The **Access** column is the lowest [API key tier]({links.To(Links.AccessTiersPage)}) that may run it.").AppendLine();

        foreach (var area in Areas.All)
        {
            text.AppendLine($"## [{area.Title}]({area.Folder}/index.md)").AppendLine();
            text.AppendLine(AreaCommands(area, catalog, links)).AppendLine();
        }

        return text.ToString().TrimEnd() + "\n";
    }

    public static string AreaLandingTable(Area area, Catalog catalog, Links links) => AreaCommands(area, catalog, links);

    public static string Tiers(Catalog catalog, Links links)
    {
        var text = new StringBuilder("| Tier | Commands |\n|---|---|\n");

        foreach (var access in new[] { RelayAccess.Read, RelayAccess.Write, RelayAccess.Admin })
        {
            var verbs = catalog.Commands
                .Where(c => c.Access == access)
                .OrderBy(c => c.Verb, StringComparer.Ordinal)
                .Select(c => links.Command(c));

            var includes = access == RelayAccess.Read ? string.Empty : $"Everything in {(access == RelayAccess.Write ? "Read" : "Write")}, plus: ";
            text.AppendLine($"| **{access}** | {includes}{string.Join(", ", verbs)} |");
        }

        return text.ToString().TrimEnd();
    }

    public static string TypesPage(Catalog catalog)
    {
        var links = new Links(catalog, Links.TypesPage);
        var text = new StringBuilder();

        text.AppendLine("# Types").AppendLine();
        text.AppendLine("The JSON shapes Relay sends and receives. Property names are camelCase on the wire. In the tables, `guid` is a GUID written as a string, `datetime` is an ISO 8601 date-time string, " +
                        "`any` is any JSON value, `T[]` is an array, and `{ name: T }` is an object whose property names are chosen by the caller.").AppendLine();

        var envelopeTypes = new[] { typeof(RelayCommandEnvelope), typeof(RelayCommandResult), typeof(RelayBatchRequest), typeof(RelayBatchResponse), typeof(RelayDiscoveryResponse), typeof(RelayVerbDescriptor) };
        var groups = new (string Title, string Intro, IEnumerable<TypeModel> Types)[]
        {
            ("Envelopes and results", "What every call sends and every answer carries.", catalog.Types.Where(t => envelopeTypes.Contains(t.Type))),
            ("Shared input types", "A type that is part of the parameters of more than one command.", catalog.Types.Where(t => t.Type == typeof(Contracts.Commands.RelayAsset))),
            ("Command results", "What a successful command returns in `data`. Each command page says which one.", catalog.Types.Where(t => !envelopeTypes.Contains(t.Type) && t.Type != typeof(Contracts.Commands.RelayAsset))),
        };

        foreach (var (title, intro, types) in groups)
        {
            text.AppendLine($"## {title}").AppendLine().AppendLine(intro).AppendLine();

            foreach (var type in types)
            {
                text.AppendLine($"### {type.Name}").AppendLine();

                var description = Description(type.Summary, links);
                if (description.Length > 0)
                {
                    text.AppendLine(description).AppendLine();
                }

                // The summary often says which command returns the type already.
                var usedBy = catalog.Commands.Where(c => c.ResultType == type.Type && !description.Contains($"`{c.Verb}`", StringComparison.Ordinal)).ToList();
                if (usedBy.Count > 0)
                {
                    text.AppendLine("Returned by " + string.Join(", ", usedBy.Select(links.Command)) + ".").AppendLine();
                }

                text.AppendLine(PropertyTable(type, catalog, links, "Field")).AppendLine();
            }
        }

        return text.ToString().TrimEnd() + "\n";
    }

    public static string NavCommands(Catalog catalog, string indent)
    {
        var text = new StringBuilder();
        text.AppendLine($"{indent}- commands/index.md");

        foreach (var area in Areas.All)
        {
            text.AppendLine($"{indent}- {area.Title}:");
            text.AppendLine($"{indent}  - commands/{area.Folder}/index.md");

            foreach (var verb in area.Verbs)
            {
                text.AppendLine($"{indent}  - commands/{area.Folder}/{verb}.md");
            }
        }

        return text.ToString().TrimEnd();
    }

    // ---------------------------------------------------------------------------------------------------------------
    // Helpers

    private static string PropertyTable(TypeModel model, Catalog catalog, Links links, string firstColumn)
    {
        var text = new StringBuilder();
        text.AppendLine($"| {firstColumn} | Type | Description |").AppendLine("|---|---|---|");

        foreach (var property in model.Properties)
        {
            text.AppendLine($"| `{property.JsonName}` | {TypeText.Of(property.Type, catalog, links)} | {Description(property.Summary, links)} |");
        }

        return text.ToString().TrimEnd();
    }

    /// <summary>A parameter is required when its description says so (as a sentence of its own, not "Required when ...").</summary>
    private static bool IsRequired(string description) => RequiredMarker().IsMatch(description);

    private static string Curl(string verb, System.Text.Json.JsonElement parameters)
    {
        var body = ExampleStore.Envelope(verb, parameters).Replace("'", "'\\''");

        return "curl -X POST \"https://your-site.example/api/relay/commands\" \\\n" +
               "  -H \"X-Relay-Api-Key: YOUR_API_KEY\" \\\n" +
               "  -H \"Content-Type: application/json\" \\\n" +
               $"  -d '{body}'";
    }

    private static string Fence(string language, string code) => $"```{language}\n{code.TrimEnd()}\n```";

    private static string Indent(string text) =>
        string.Join("\n", text.Split('\n').Select(line => line.Length == 0 ? line : "    " + line));

    /// <summary>The table has a Required column, so the "Required." of the XML summary (which IntelliSense shows) is dropped from the description.</summary>
    private static string WithoutRequiredMarker(string description)
    {
        var cleaned = RequiredSentence().Replace(description, string.Empty).Trim();
        return RequiredWithRest().Replace(cleaned, match => match.Groups["rest"].Value.ToUpperInvariant());
    }

    [GeneratedRegex(@"(?<![Nn]ot )\bRequired\b(?! when| if)")]
    private static partial Regex RequiredMarker();

    [GeneratedRegex(@"\s*(?<![Nn]ot )\bRequired\.(?=\s|$)")]
    private static partial Regex RequiredSentence();

    [GeneratedRegex(@"\bRequired[;:]\s+(?<rest>.)")]
    private static partial Regex RequiredWithRest();
}
