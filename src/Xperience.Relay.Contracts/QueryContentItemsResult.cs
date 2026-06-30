using System.Text.Json;

namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "query-content-items" command.
/// Each item is a dictionary of column name to its JSON-serialized value.
/// </summary>
public class QueryContentItemsResult
{
    public List<Dictionary<string, JsonElement>> Items { get; set; } = [];
}
