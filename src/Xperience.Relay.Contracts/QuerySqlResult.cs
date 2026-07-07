namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "query-sql" command.
/// </summary>
public class QuerySqlResult
{
    public List<string> Columns { get; set; } = [];
    public List<List<string?>> Rows { get; set; } = [];
}
