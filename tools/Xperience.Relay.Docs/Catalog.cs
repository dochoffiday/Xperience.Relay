using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xperience.Relay.Contracts;

namespace Xperience.Relay.Docs;

sealed class PropertyModel
{
    public required PropertyInfo Property { get; init; }
    public required XElement? Summary { get; init; }

    /// <summary>The value a freshly constructed command has for this property (null for result types).</summary>
    public object? DefaultValue { get; init; }

    public string Name => Property.Name;

    /// <summary>Property name as it travels on the wire (the relay uses camelCase).</summary>
    public string JsonName => JsonNamingPolicy.CamelCase.ConvertName(Property.Name);

    public Type Type => Property.PropertyType;
}

class TypeModel
{
    public required Type Type { get; init; }
    public required XElement? Summary { get; init; }
    public required IReadOnlyList<PropertyModel> Properties { get; init; }

    public string Name => Type.Name;

    /// <summary>The id the Markdown toc extension gives a heading that is just the type name.</summary>
    public string Anchor => Name.ToLowerInvariant();
}

sealed class CommandModel : TypeModel
{
    public required string Verb { get; init; }
    public required RelayAccess Access { get; init; }
    public required Area Area { get; init; }

    /// <summary>The DTO a successful run returns in <c>data</c>, or null when it returns none.</summary>
    public Type? ResultType { get; set; }

    /// <summary>
    /// Path of the page the command is documented on, relative to the docs folder. Every command of an area shares the
    /// area's page, as a section whose heading is the verb.
    /// </summary>
    public string PagePath => $"commands/{Area.Folder}/index.md";
}

/// <summary>Everything the generator knows about the Contracts package, read by reflection and from its XML docs.</summary>
sealed class Catalog
{
    private static readonly Regex ReturnsPattern = new(
        @"<see\s+cref=""T:(?<type>[^""]+)""\s*/>\s*in\s*<see\s+cref=""P:Xperience\.Relay\.Contracts\.RelayCommandResult\.Data""\s*/>",
        RegexOptions.Compiled);

    public required IReadOnlyList<CommandModel> Commands { get; init; }

    /// <summary>The non-command types of the wire contract: envelopes, shared input types and result DTOs.</summary>
    public required IReadOnlyList<TypeModel> Types { get; init; }

    public CommandModel? CommandFor(Type type) => Commands.FirstOrDefault(c => c.Type == type);

    public TypeModel? TypeFor(Type type) => Types.FirstOrDefault(t => t.Type == type);

    public static Catalog Build(Assembly contracts, XmlDocs docs, List<string> problems)
    {
        var commandTypes = contracts.GetExportedTypes()
            .Where(t => t.GetCustomAttribute<RelayCommandAttribute>() is not null)
            .OrderBy(t => t.GetCustomAttribute<RelayCommandAttribute>()!.Verb, StringComparer.Ordinal)
            .ToList();

        var commands = new List<CommandModel>();
        foreach (var type in commandTypes)
        {
            var attribute = type.GetCustomAttribute<RelayCommandAttribute>()!;
            var area = Areas.ForVerb(attribute.Verb);
            if (area is null)
            {
                problems.Add($"'{attribute.Verb}' has no area: add it to Areas.cs so it gets a page.");
                continue;
            }

            var instance = Activator.CreateInstance(type);
            var command = new CommandModel
            {
                Type = type,
                Summary = docs.SummaryOf(type),
                Properties = ReadProperties(type, docs, instance),
                Verb = attribute.Verb,
                Access = attribute.Access,
                Area = area,
            };
            command.ResultType = FindResultType(command, contracts, problems);
            commands.Add(command);
        }

        foreach (var area in Areas.All)
        {
            foreach (var verb in area.Verbs.Where(v => commands.All(c => c.Verb != v)))
            {
                problems.Add($"Areas.cs lists '{verb}' under '{area.Title}', but no command has that verb.");
            }
        }

        return new Catalog { Commands = commands, Types = ReadTypes(contracts, commands, docs) };
    }

    private static List<PropertyModel> ReadProperties(Type type, XmlDocs docs, object? instance)
    {
        // Base class properties first, each class in declaration order.
        var chain = new List<Type>();
        for (var t = type; t is not null && t != typeof(object); t = t.BaseType)
        {
            chain.Insert(0, t);
        }

        return chain
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
            .Select(p => new PropertyModel { Property = p, Summary = docs.SummaryOf(p), DefaultValue = instance is null ? null : p.GetValue(instance) })
            .ToList();
    }

    private static Type? FindResultType(CommandModel command, Assembly contracts, List<string> problems)
    {
        if (command.Summary is null)
        {
            problems.Add($"{command.Type.Name} has no XML summary.");
            return null;
        }

        var match = ReturnsPattern.Match(command.Summary.ToString());
        if (!match.Success)
        {
            return null;
        }

        var resultType = contracts.GetType(match.Groups["type"].Value);
        if (resultType is null)
        {
            problems.Add($"{command.Type.Name}'s summary says it returns {match.Groups["type"].Value}, which isn't a type in Contracts.");
        }

        return resultType;
    }

    private static List<TypeModel> ReadTypes(Assembly contracts, List<CommandModel> commands, XmlDocs docs)
    {
        var shared = new List<Type>
        {
            typeof(RelayCommandEnvelope), typeof(RelayCommandResult), typeof(RelayBatchRequest), typeof(RelayBatchResponse),
            typeof(RelayDiscoveryResponse), typeof(RelayVerbDescriptor), typeof(Contracts.Commands.RelayAsset),
        };

        // Result DTOs: every type a command returns, plus the types nested in their properties.
        var results = new List<Type>();
        void Visit(Type type)
        {
            var inner = Unwrap(type);
            if (inner.Assembly != contracts || inner.IsEnum || shared.Contains(inner) || results.Contains(inner))
            {
                return;
            }

            results.Add(inner);
            foreach (var property in inner.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Visit(property.PropertyType);
            }
        }

        foreach (var result in commands.Select(c => c.ResultType).Where(t => t is not null))
        {
            Visit(result!);
        }

        // WebPageData/ContentData derive from the *Info types, which are documented too.
        foreach (var result in results.ToList())
        {
            if (result.BaseType is { } baseType && baseType != typeof(object))
            {
                Visit(baseType);
            }
        }

        return shared.Concat(results.OrderBy(t => t.Name, StringComparer.Ordinal))
            .Select(t => new TypeModel { Type = t, Summary = docs.SummaryOf(t), Properties = ReadProperties(t, docs, null) })
            .ToList();
    }

    /// <summary>Strips nullable, list and dictionary wrappers to get at the type that matters.</summary>
    public static Type Unwrap(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type.IsGenericType)
        {
            return Unwrap(type.GetGenericArguments().Last());
        }

        return type;
    }
}
