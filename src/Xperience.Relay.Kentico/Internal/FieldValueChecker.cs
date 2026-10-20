using System.Globalization;
using CMS.DataEngine;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>What the relay needs to know about one field of a content type to check a value before Kentico sees it.</summary>
internal sealed record FieldRule(string Name, string DataType, int Size, int Precision, bool AllowEmpty);

/// <summary>The kinds of field a command sets through its own typed property rather than as a plain value.</summary>
internal enum FieldUse
{
    LinkedItems,
    Tags,
    Asset,
}

/// <summary>A field set through <see cref="FieldUse"/>, which has to be a field of the matching kind.</summary>
internal sealed record TypedField(string Name, FieldUse Use);

/// <summary>
/// Checks field names and values against a content type's fields. Kentico drops a field it doesn't know without saying
/// so, stores linked items or tags in whatever field it is told to (as JSON text, in a text field), and fails on a value
/// it can't store only halfway through the write, after the draft exists. All of that is caught here instead. Types the
/// relay can't judge from a JSON value (references, assets, tags, ...) are not checked beyond the field existing.
/// </summary>
internal static class FieldValueChecker
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <param name="contentTypeName">Used in the messages.</param>
    /// <param name="rules">Every field of the content type, including those of its reusable field schemas.</param>
    /// <param name="plainValues">Plain field values, as they will be handed to Kentico.</param>
    /// <param name="typedFields">Fields set through linked items, tags or assets, which must be fields of that kind.</param>
    /// <returns>Null when everything is fine, otherwise every problem found.</returns>
    public static string? Check(
        string contentTypeName,
        IEnumerable<FieldRule> rules,
        IEnumerable<KeyValuePair<string, object?>> plainValues,
        IEnumerable<TypedField> typedFields)
    {
        var fields = new Dictionary<string, FieldRule>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in rules)
        {
            fields.TryAdd(rule.Name, rule);
        }

        var values = plainValues.ToList();
        var typed = typedFields.ToList();
        var problems = new List<string>();

        var unknown = values.Select(v => v.Key)
            .Concat(typed.Select(t => t.Name))
            .Where(name => !fields.ContainsKey(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => $"'{name}'")
            .ToList();

        if (unknown.Count > 0)
        {
            problems.Add(unknown.Count == 1
                ? $"Field {unknown[0]} was not found on content type '{contentTypeName}'."
                : $"Fields {string.Join(", ", unknown)} were not found on content type '{contentTypeName}'.");
        }

        foreach (var (name, value) in values)
        {
            if (fields.TryGetValue(name, out var rule) && CheckValue(rule, value) is { } problem)
            {
                problems.Add(problem);
            }
        }

        foreach (var (name, use) in typed)
        {
            if (fields.TryGetValue(name, out var rule) && CheckUse(rule, use) is { } problem)
            {
                problems.Add(problem);
            }
        }

        return problems.Count == 0 ? null : string.Join(" ", problems);
    }

    private static string? CheckUse(FieldRule field, FieldUse use)
    {
        var (dataType, what, property) = use switch
        {
            FieldUse.LinkedItems => (FieldDataType.ContentItemReference, "a content item reference field", "LinkedItemFields"),
            FieldUse.Tags => (FieldDataType.Taxonomy, "a tag field", "TagFields"),
            _ => (FieldDataType.ContentItemAsset, "an asset field", "Assets"),
        };

        return string.Equals(field.DataType, dataType, StringComparison.OrdinalIgnoreCase)
            ? null
            : $"Field '{field.Name}' is not {what} (it is {field.DataType}), so it can't be set from {property}.";
    }

    private static string? CheckValue(FieldRule field, object? value)
    {
        // A field that must have a value is a column that doesn't allow NULL, which SQL refuses mid-write.
        if (value is null)
        {
            return field.AllowEmpty ? null : $"Field '{field.Name}' can't be null.";
        }

        return field.DataType.ToLowerInvariant() switch
        {
            FieldDataType.Text => CheckText(field, value),
            FieldDataType.Integer => CheckWholeNumber(field, value, int.MinValue, int.MaxValue),
            FieldDataType.LongInteger => CheckWholeNumber(field, value, long.MinValue, long.MaxValue),
            FieldDataType.Double => CheckNumber(field, value, s => double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, Invariant, out _)),
            FieldDataType.Decimal => CheckDecimal(field, value),
            FieldDataType.Boolean => CheckBoolean(field, value),
            FieldDataType.DateTime or FieldDataType.Date => Expect(field, "a date", value is DateTime or DateTimeOffset || value is string d && DateTime.TryParse(d, Invariant, DateTimeStyles.None, out _), value),
            FieldDataType.TimeSpan => Expect(field, "a time span", value is TimeSpan || value is string t && TimeSpan.TryParse(t, Invariant, out _), value),
            FieldDataType.Guid => Expect(field, "a GUID", value is Guid || value is string g && Guid.TryParse(g, out _), value),
            _ => null,
        };
    }

    private static string? CheckText(FieldRule field, object value) =>
        value is string text && field.Size > 0 && text.Length > field.Size
            ? $"Field '{field.Name}' allows at most {field.Size} characters, but the value has {text.Length}."
            : null;

    // Kentico (through SQL) would round a fractional number into a whole-number column, so that is refused too.
    private static string? CheckWholeNumber(FieldRule field, object value, long min, long max)
    {
        var fits = value switch
        {
            int i => i >= min && i <= max,
            long l => l >= min && l <= max,
            decimal d => d == decimal.Truncate(d) && d >= min && d <= max,
            double d => d == Math.Truncate(d) && d >= min && d <= max,
            string s => long.TryParse(s, NumberStyles.Integer, Invariant, out var parsed) && parsed >= min && parsed <= max,
            _ => false,
        };

        return Expect(field, "a whole number", fits, value);
    }

    // A decimal column holds Size digits in all, Precision of them after the point; SQL fails on more digits before it.
    private static string? CheckDecimal(FieldRule field, object value)
    {
        if (!TryAsDecimal(value, out var number))
        {
            return Expect(field, "a number", false, value);
        }

        var wholeDigits = field.Size - field.Precision;

        return field.Size > 0 && wholeDigits is > 0 and < 28 && Math.Abs(number) >= Pow10(wholeDigits)
            ? $"Field '{field.Name}' allows at most {wholeDigits} digits before the decimal point, but got {Describe(value)}."
            : null;
    }

    private static bool TryAsDecimal(object value, out decimal number)
    {
        number = 0;

        switch (value)
        {
            case int i:
                number = i;
                return true;
            case long l:
                number = l;
                return true;
            case decimal d:
                number = d;
                return true;
            case double d when d >= (double)decimal.MinValue && d <= (double)decimal.MaxValue:
                number = (decimal)d;
                return true;
            case string s:
                return decimal.TryParse(s, NumberStyles.Number, Invariant, out number);
            default:
                return false;
        }
    }

    private static decimal Pow10(int digits)
    {
        var result = 1m;

        for (var i = 0; i < digits; i++)
        {
            result *= 10;
        }

        return result;
    }

    private static string? CheckNumber(FieldRule field, object value, Func<string, bool> parses) =>
        Expect(field, "a number", value is int or long or decimal or double || value is string s && parses(s), value);

    private static string? CheckBoolean(FieldRule field, object value)
    {
        var isBoolean = value switch
        {
            bool => true,
            string s => bool.TryParse(s, out _),
            int i => i is 0 or 1,
            long l => l is 0 or 1,
            decimal d => d == 0 || d == 1,
            double d => d == 0 || d == 1,
            _ => false,
        };

        return Expect(field, "true or false", isBoolean, value);
    }

    private static string? Expect(FieldRule field, string expected, bool valid, object value) =>
        valid ? null : $"Field '{field.Name}' expects {expected}, but got {Describe(value)}.";

    private static string Describe(object value) => value switch
    {
        string s => s.Length > 50 ? $"'{s[..50]}...'" : $"'{s}'",
        bool b => b ? "true" : "false",
        IFormattable f => f.ToString(null, Invariant),
        _ => value.ToString() ?? string.Empty,
    };
}
