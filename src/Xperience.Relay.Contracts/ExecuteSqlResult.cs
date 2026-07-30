namespace Xperience.Relay.Contracts;

public class ExecuteSqlResult
{
    /// <summary>True when the query was actually executed; false when this is a preview.</summary>
    public bool Confirmed { get; set; }

    /// <summary>
    /// One entry per statement describing the verb and tables involved (e.g. "UPDATE [CMS_User]").
    /// Populated in both preview and execute modes.
    /// </summary>
    public List<string> StatementSummaries { get; set; } = [];

    /// <summary>Column names, when execution returned a result set.</summary>
    public List<string>? Columns { get; set; }

    /// <summary>Row data, when execution returned a result set.</summary>
    public List<List<string?>>? Rows { get; set; }
}
