namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// Checks the columns a custom object command sets against the columns of the object type. Kentico accepts a column the
/// type doesn't have and drops it without saying so, so a misspelled column would look like it was saved.
/// </summary>
internal static class ObjectColumnChecker
{
    /// <param name="objectTypeName">Used in the message.</param>
    /// <param name="knownColumns">The columns of the object type. A type that doesn't list its columns isn't checked.</param>
    /// <param name="columns">The columns the command sets.</param>
    /// <returns>Null when every column exists, otherwise a message naming those that don't.</returns>
    public static string? Check(string objectTypeName, IReadOnlyCollection<string> knownColumns, IEnumerable<string> columns)
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
            1 => $"Column {unknown[0]} was not found on object type '{objectTypeName}'.",
            _ => $"Columns {string.Join(", ", unknown)} were not found on object type '{objectTypeName}'.",
        };
    }
}
