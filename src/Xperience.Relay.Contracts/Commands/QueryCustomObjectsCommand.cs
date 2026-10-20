using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Reads objects of any registered Kentico object type (typically a custom module class). Every value comes back as a
/// string. The relay's own API key object type is refused. Returns a <see cref="QueryCustomObjectsResult"/> in
/// <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("query-custom-objects", Access = RelayAccess.Admin)]
public class QueryCustomObjectsCommand : IRelayCommand
{
    /// <summary>Registered object type name, e.g. <c>custom.redirect</c>. Required.</summary>
    public string ObjectTypeName { get; set; } = string.Empty;

    /// <summary>Columns to return. All of the object's columns when omitted or empty.</summary>
    public List<string>? Columns { get; set; }

    /// <summary>Optional column-equality filters. All conditions are ANDed together.</summary>
    public Dictionary<string, JsonElement>? WhereEquals { get; set; }
}
