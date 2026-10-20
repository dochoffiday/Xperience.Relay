namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Runs SQL against the Xperience database in two steps. Without <see cref="Confirm"/> the SQL is only validated and
/// described, so the caller can review what would run. With <see cref="Confirm"/> it runs. Besides SELECT and DECLARE
/// it allows INSERT, UPDATE, DELETE, TRUNCATE TABLE, EXECUTE and any CREATE, ALTER or DROP statement. A few dangerous
/// functions are always refused, as is anything that mentions the relay's API key table. This cannot be undone for
/// data and schema changes. Returns an <see cref="ExecuteSqlResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("execute-sql", Access = RelayAccess.Admin)]
public class ExecuteSqlCommand : IRelayCommand
{
    /// <summary>The SQL to validate or run. May hold several statements. A trailing semicolon is ignored. Required.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>False (the default) only previews the statements; true runs them.</summary>
    public bool Confirm { get; set; }
}
