namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Executes a read-only SQL query against the Xperience database. Only SELECT (including WITH ... SELECT) and DECLARE
/// statements are permitted, checked with a T-SQL parser rather than keyword matching. A few dangerous functions are
/// always refused, as is anything that mentions the relay's API key table. Only the first result set is returned.
/// Results land in <see cref="QuerySqlResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("query-sql", Access = RelayAccess.Admin)]
public class QuerySqlCommand : IRelayCommand
{
    /// <summary>The SQL query to execute. May hold several statements. A trailing semicolon is ignored. Required.</summary>
    public string Query { get; set; } = string.Empty;
}
