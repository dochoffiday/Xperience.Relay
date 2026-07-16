using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("query-custom-objects")]
public class QueryCustomObjectsCommand : IRelayCommand
{
    public string ObjectTypeName { get; set; } = string.Empty;
    public List<string>? Columns { get; set; }
    public Dictionary<string, JsonElement>? WhereEquals { get; set; }
}
