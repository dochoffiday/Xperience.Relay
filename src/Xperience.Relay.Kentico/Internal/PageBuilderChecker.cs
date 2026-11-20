using System.Text.Json;
using Xperience.Relay.Contracts;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// Checks the Page Builder data of a create or update command before anything is created or changed: that the widgets
/// have the structure Kentico stores (areas, sections, zones, widgets and variants, each with its identifier) and that the
/// template names an identifier. What a widget's properties hold is the widget's own business, so they are not looked
/// into. Every problem is reported at once, with where in the data it is.
/// </summary>
internal static class PageBuilderChecker
{
    /// <summary>The longest Page Builder JSON accepted, in characters. Pages with hundreds of widgets are far below it.</summary>
    public const int MaxJsonLength = 2_000_000;

    private const int MaxProblemsShown = 10;

    /// <returns>Null when the data is fine, otherwise the problems found.</returns>
    public static string? Check(PageBuilderData data)
    {
        if (data.Widgets is null && data.Template is null)
        {
            return "PageBuilder needs Widgets, Template or both.";
        }

        var problems = new List<string>();

        if (data.Widgets is { } widgets)
        {
            CheckWidgets(widgets, problems);
        }

        if (data.Template is { } template)
        {
            CheckTemplate(template, problems);
        }

        return problems.Count == 0 ? null : string.Join(" ", problems.Take(MaxProblemsShown));
    }

    /// <summary>The identifier of the template in Page Builder JSON, or null when there is none.</summary>
    public static string? TemplateIdentifier(JsonElement? template) =>
        template is { ValueKind: JsonValueKind.Object } value
        && value.TryGetProperty("identifier", out var identifier)
        && identifier.ValueKind == JsonValueKind.String
        && !string.IsNullOrWhiteSpace(identifier.GetString())
            ? identifier.GetString()
            : null;

    private static void CheckWidgets(JsonElement widgets, List<string> problems)
    {
        if (widgets.GetRawText().Length > MaxJsonLength)
        {
            problems.Add($"PageBuilder.Widgets is longer than {MaxJsonLength} characters.");
            return;
        }

        if (widgets.ValueKind != JsonValueKind.Object
            || !widgets.TryGetProperty("editableAreas", out var areas)
            || areas.ValueKind != JsonValueKind.Array)
        {
            problems.Add("PageBuilder.Widgets must be an object with an 'editableAreas' list.");
            return;
        }

        var identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var index = 0;

        foreach (var area in areas.EnumerateArray())
        {
            CheckArea(area, $"Widgets.editableAreas[{index++}]", identifiers, problems);
        }
    }

    private static void CheckArea(JsonElement area, string path, HashSet<string> identifiers, List<string> problems)
    {
        if (!IsObject(area, path, problems))
        {
            return;
        }

        if (GetString(area, "identifier") is null)
        {
            problems.Add($"{path}: 'identifier' is required.");
        }

        foreach (var (section, sectionPath) in Items(area, "sections", path, problems))
        {
            CheckSection(section, sectionPath, identifiers, problems);
        }
    }

    private static void CheckSection(JsonElement section, string path, HashSet<string> identifiers, List<string> problems)
    {
        if (!IsObject(section, path, problems))
        {
            return;
        }

        CheckIdentifier(section, path, identifiers, problems);
        CheckType(section, path, problems);
        CheckOptionalObject(section, "properties", path, problems);
        CheckOptionalObject(section, "fieldIdentifiers", path, problems);

        foreach (var (zone, zonePath) in Items(section, "zones", path, problems))
        {
            CheckZone(zone, zonePath, identifiers, problems);
        }
    }

    private static void CheckZone(JsonElement zone, string path, HashSet<string> identifiers, List<string> problems)
    {
        if (!IsObject(zone, path, problems))
        {
            return;
        }

        CheckIdentifier(zone, path, identifiers, problems);

        foreach (var (widget, widgetPath) in Items(zone, "widgets", path, problems))
        {
            CheckWidget(widget, widgetPath, identifiers, problems);
        }
    }

    private static void CheckWidget(JsonElement widget, string path, HashSet<string> identifiers, List<string> problems)
    {
        if (!IsObject(widget, path, problems))
        {
            return;
        }

        CheckIdentifier(widget, path, identifiers, problems);
        CheckType(widget, path, problems);

        var variants = Items(widget, "variants", path, problems).ToList();

        if (variants.Count == 0 && widget.TryGetProperty("variants", out var list) && list.ValueKind == JsonValueKind.Array)
        {
            problems.Add($"{path}: 'variants' needs at least one variant.");
        }

        foreach (var (variant, variantPath) in variants)
        {
            if (!IsObject(variant, variantPath, problems))
            {
                continue;
            }

            CheckIdentifier(variant, variantPath, identifiers, problems);
            CheckOptionalObject(variant, "properties", variantPath, problems);
            CheckOptionalObject(variant, "fieldIdentifiers", variantPath, problems);
        }
    }

    private static void CheckTemplate(JsonElement template, List<string> problems)
    {
        if (template.GetRawText().Length > MaxJsonLength)
        {
            problems.Add($"PageBuilder.Template is longer than {MaxJsonLength} characters.");
            return;
        }

        if (template.ValueKind != JsonValueKind.Object)
        {
            problems.Add("PageBuilder.Template must be an object with the template's 'identifier'.");
            return;
        }

        if (GetString(template, "identifier") is null)
        {
            problems.Add("PageBuilder.Template: 'identifier' is required.");
        }

        CheckOptionalObject(template, "properties", "Template", problems);
        CheckOptionalObject(template, "fieldIdentifiers", "Template", problems);
    }

    private static bool IsObject(JsonElement element, string path, List<string> problems)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            return true;
        }

        problems.Add($"{path} must be an object.");
        return false;
    }

    /// <summary>The entries of a list the structure requires, with their paths. A missing or misshapen list is a problem.</summary>
    private static IEnumerable<(JsonElement Item, string Path)> Items(JsonElement parent, string name, string path, List<string> problems)
    {
        if (!parent.TryGetProperty(name, out var list) || list.ValueKind != JsonValueKind.Array)
        {
            problems.Add($"{path}: '{name}' must be a list.");
            return [];
        }

        var index = 0;
        return list.EnumerateArray().Select(item => (item, $"{path}.{name}[{index++}]")).ToList();
    }

    private static void CheckIdentifier(JsonElement element, string path, HashSet<string> identifiers, List<string> problems)
    {
        var identifier = GetString(element, "identifier");

        if (identifier is null || !Guid.TryParse(identifier, out var guid))
        {
            problems.Add($"{path}: 'identifier' must be a GUID.");
        }
        else if (!identifiers.Add(guid.ToString()))
        {
            problems.Add($"{path}: the identifier {guid} is used more than once.");
        }
    }

    private static void CheckType(JsonElement element, string path, List<string> problems)
    {
        if (GetString(element, "type") is null)
        {
            problems.Add($"{path}: 'type' is required.");
        }
    }

    /// <summary>A property that may be left out or null, but when it is there has to be an object.</summary>
    private static void CheckOptionalObject(JsonElement element, string name, string path, List<string> problems)
    {
        if (element.TryGetProperty(name, out var value) && value.ValueKind is not (JsonValueKind.Object or JsonValueKind.Null))
        {
            problems.Add($"{path}: '{name}' must be an object or null.");
        }
    }

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()
            : null;
}
