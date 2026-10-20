namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "query-custom-objects" command.</summary>
public class QueryCustomObjectsResult
{
    /// <summary>One dictionary per object, mapping column name to its value as text (null for SQL NULL).</summary>
    public List<Dictionary<string, string?>> Items { get; set; } = [];
}
