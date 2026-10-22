using System.Text.Json;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// Checks the names and sizes a query command hands over, before they are used in a query. Kentico takes a column as an
/// expression rather than as a name, so a column has to be plainly a name by the time it gets there. What isn't is refused,
/// never cleaned up, and every problem is reported at once, as the checks on content writes do.
/// </summary>
internal static class QueryInputChecker
{
    /// <summary>The longest name SQL Server accepts.</summary>
    public const int MaxNameLength = 128;

    /// <summary>The longest code name of a content type or form.</summary>
    public const int MaxCodeNameLength = 100;

    public const int MaxContentTypes = 25;
    public const int MaxColumns = 500;
    public const int MaxFilters = 50;
    public const int MaxSearchTextLength = 1000;

    private const int MaxNamesShown = 5;
    private const int MaxNameShownLength = 40;

    /// <summary>A column or field name: ASCII letters, digits and underscores, not starting with a digit.</summary>
    public static bool IsName(string? value) =>
        value is { Length: > 0 and <= MaxNameLength } && IsNameStart(value[0]) && value.All(IsNameCharacter);

    /// <summary>A code name such as <c>DancingGoat.ArticlePage</c>: one or more names, separated by dots.</summary>
    public static bool IsCodeName(string? value) =>
        value is { Length: > 0 and <= MaxCodeNameLength } && value.Split('.').All(IsName);

    /// <summary>Checks everything a query command takes, apart from whether the names exist.</summary>
    /// <returns>Null when everything is fine, otherwise every problem found.</returns>
    public static string? CheckQuery(
        IReadOnlyCollection<string?> contentTypeNames,
        IReadOnlyCollection<string?> columns,
        IReadOnlyDictionary<string, JsonElement>? whereEquals) =>
        Join(CheckContentTypeNames(contentTypeNames), CheckColumnNames("Columns", columns), CheckWhereEquals(whereEquals));

    public static string? CheckContentTypeNames(IReadOnlyCollection<string?> names)
    {
        // A list over its limit is refused without looking at what is in it.
        if (names.Count > MaxContentTypes)
        {
            return $"ContentTypeNames can list at most {MaxContentTypes} content types.";
        }

        var invalid = names.Where(name => !IsCodeName(name)).ToList();

        if (invalid.Count == 0)
        {
            return null;
        }

        return invalid.Count == 1
            ? $"{Quote(invalid)} is not a content type name; use the code name, such as DancingGoat.ArticlePage."
            : $"{Quote(invalid)} are not content type names; use the code names, such as DancingGoat.ArticlePage.";
    }

    /// <param name="parameter">The name of the parameter that lists the columns, used in the message about too many.</param>
    /// <param name="names">The column names to check.</param>
    public static string? CheckColumnNames(string parameter, IReadOnlyCollection<string?> names)
    {
        if (names.Count > MaxColumns)
        {
            return $"{parameter} can list at most {MaxColumns} columns.";
        }

        var invalid = names.Where(name => !IsName(name)).ToList();

        if (invalid.Count == 0)
        {
            return null;
        }

        return invalid.Count == 1
            ? $"Column name {Quote(invalid)} isn't valid: use letters, digits and underscores only."
            : $"Column names {Quote(invalid)} aren't valid: use letters, digits and underscores only.";
    }

    /// <summary>Checks the column names of a <c>WhereEquals</c> filter, and that each value is a plain value.</summary>
    public static string? CheckWhereEquals(IReadOnlyDictionary<string, JsonElement>? whereEquals)
    {
        if (whereEquals is null)
        {
            return null;
        }

        if (whereEquals.Count > MaxFilters)
        {
            return $"WhereEquals can have at most {MaxFilters} entries.";
        }

        var problems = new List<string?> { CheckColumnNames("WhereEquals", whereEquals.Keys.ToList()) };

        // A value that isn't plain would be read as null, so the filter would quietly match something else.
        var notPlain = whereEquals
            .Where(entry => entry.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True
                or JsonValueKind.False or JsonValueKind.Null))
            .Select(entry => entry.Key)
            .ToList();

        if (notPlain.Count > 0)
        {
            problems.Add(notPlain.Count == 1
                ? $"The WhereEquals value for {Quote(notPlain)} must be text, a number, true, false or null."
                : $"The WhereEquals values for {Quote(notPlain)} must be text, numbers, true, false or null.");
        }

        return Join(problems.ToArray());
    }

    public static string? CheckSearchText(string? text) =>
        text is { Length: > MaxSearchTextLength } ? $"Filter can be at most {MaxSearchTextLength} characters." : null;

    private static string? Join(params string?[] problems)
    {
        var found = problems.Where(problem => !string.IsNullOrEmpty(problem)).ToList();
        return found.Count == 0 ? null : string.Join(" ", found);
    }

    /// <summary>The values as they are shown in a message: a few of them, each cut short, with nothing unprintable.</summary>
    private static string Quote(IReadOnlyCollection<string?> values)
    {
        var distinct = values.Select(value => value ?? string.Empty).Distinct().ToList();
        var shown = distinct.Take(MaxNamesShown).Select(value => $"'{Printable(value)}'");
        var more = distinct.Count - MaxNamesShown;

        return string.Join(", ", shown) + (more > 0 ? $" and {more} more" : string.Empty);
    }

    /// <summary>Text as it is shown in a message: cut short, and with nothing unprintable in it.</summary>
    public static string Printable(string value)
    {
        var text = value.Length > MaxNameShownLength ? value[..MaxNameShownLength] + "..." : value;
        return new string(text.Select(c => char.IsControl(c) ? '?' : c).ToArray());
    }

    private static bool IsNameStart(char c) => c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_';

    private static bool IsNameCharacter(char c) => IsNameStart(c) || c is >= '0' and <= '9';
}
