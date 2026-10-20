using System.Text.Json;
using System.Text.RegularExpressions;

namespace Xperience.Relay.Docs;

/// <summary>Writes links between pages of the docs, relative to the page they are written on.</summary>
sealed partial class Links(Catalog catalog, string fromPage) : IMarkdownLinks
{
    public const string TypesPage = "reference/types.md";
    public const string ConfigurationPage = "configuration.md";
    public const string AccessTiersPage = "concepts/access-tiers.md";

    private readonly string _fromDirectory = Path.GetDirectoryName(fromPage)?.Replace('\\', '/') ?? string.Empty;

    /// <summary>The path of <paramref name="docsPath"/> as a link from this page.</summary>
    public string To(string docsPath)
    {
        return Path.GetRelativePath(_fromDirectory.Length == 0 ? "." : _fromDirectory, docsPath).Replace('\\', '/');
    }

    public string ToCommand(CommandModel command) => To(command.PagePath);

    public string ToType(TypeModel type) => $"{To(TypesPage)}#{type.Anchor}";

    /// <summary>A link to a command's page, labelled with its verb.</summary>
    public string Command(CommandModel command) => $"[`{command.Verb}`]({ToCommand(command)})";

    public string Cref(string cref)
    {
        if (cref.Length < 3 || cref[1] != ':')
        {
            return $"`{cref}`";
        }

        var name = cref[2..];
        var shortName = name[(name.LastIndexOf('.') + 1)..];

        switch (cref[0])
        {
            case 'T':
                if (catalog.Commands.FirstOrDefault(c => c.Type.FullName == name) is { } command)
                {
                    return Command(command);
                }

                if (catalog.Types.FirstOrDefault(t => t.Type.FullName == name) is { } type)
                {
                    return $"[`{type.Name}`]({ToType(type)})";
                }

                return $"`{shortName}`";
            case 'P':
                // A property is shown the way it travels on the wire.
                return $"`{JsonNamingPolicy.CamelCase.ConvertName(shortName)}`";
            default:
                // A generic method, such as GetData``1, reads GetData<T>.
                return $"`{GenericArity().Replace(shortName, "<T>")}`";
        }
    }

    public string Code(string text)
    {
        // RelayKenticoOptions.X names a setting, which the configuration page describes.
        if (KenticoOption().Match(text) is { Success: true } option)
        {
            return $"[`{text}`]({To(ConfigurationPage)}#{option.Groups["name"].Value.ToLowerInvariant()})";
        }

        return $"`{text}`";
    }

    public string Plain(string text) =>
        // A command named in quotes, as in: use the "get-content-hub-folder" command first.
        QuotedVerb().Replace(text, match =>
            catalog.Commands.FirstOrDefault(c => c.Verb == match.Groups["verb"].Value) is { } command
                ? Command(command)
                : match.Value);

    [GeneratedRegex(@"^RelayKenticoOptions\.(?<name>\w+)$")]
    private static partial Regex KenticoOption();

    [GeneratedRegex(@"``\d+")]
    private static partial Regex GenericArity();

    [GeneratedRegex("\"(?<verb>[a-z]+(?:-[a-z]+)+)\"")]
    private static partial Regex QuotedVerb();
}
