namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "query-sql" command.
/// </summary>
public class QuerySqlResult
{
    /// <summary>Column names of the result set.</summary>
    public List<string> Columns { get; set; } = [];

    /// <summary>One list of values per row, in <see cref="Columns"/> order. Every value is text; null for SQL NULL.</summary>
    public List<List<string?>> Rows { get; set; } = [];
}
