namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// Checks the columns a command names against the columns of the type they belong to: an object type, a content type or a
/// form. Kentico accepts a column the type doesn't have and drops it without saying so, so a misspelled column would look
/// like it was saved. In a query it would only fail later, with an error from the database.
/// </summary>
internal static class ObjectColumnChecker
{
    /// <param name="typeName">Used in the message.</param>
    /// <param name="knownColumns">The columns of the type. A type that doesn't list its columns isn't checked.</param>
    /// <param name="columns">The columns the command names.</param>
    /// <param name="kind">What the type is, used in the message.</param>
    /// <returns>Null when every column exists, otherwise a message naming those that don't.</returns>
    public static string? Check(
        string typeName,
        IReadOnlyCollection<string> knownColumns,
        IEnumerable<string> columns,
        string kind = "object type")
    {
        if (knownColumns.Count == 0)
        {
            return null;
        }

        var unknown = columns
            .Where(column => !knownColumns.Contains(column, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(column => $"'{column}'")
            .ToList();

        return unknown.Count switch
        {
            0 => null,
            1 => $"Column {unknown[0]} was not found on {kind} '{typeName}'.",
            _ => $"Columns {string.Join(", ", unknown)} were not found on {kind} '{typeName}'.",
        };
    }
}
