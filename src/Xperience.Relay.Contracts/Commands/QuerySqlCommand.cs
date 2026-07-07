namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Executes a read-only SQL query against the Xperience database. Only SELECT and
/// WITH...SELECT statements are permitted — any query containing DML or DDL keywords
/// is rejected before execution. Results land in <see cref="QuerySqlResult"/> in
/// <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("query-sql")]
public class QuerySqlCommand : IRelayCommand
{
    /// <summary>The SQL query to execute. Must be a SELECT or WITH...SELECT statement.</summary>
    public string Query { get; set; } = string.Empty;
}
