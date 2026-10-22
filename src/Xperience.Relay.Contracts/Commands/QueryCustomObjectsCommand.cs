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

    /// <summary>
    /// Columns to return, each a plain column name (letters, digits and underscores) of the object type. All of the
    /// object's columns when omitted or empty.
    /// </summary>
    public List<string>? Columns { get; set; }

    /// <summary>
    /// Optional column-equality filters. All conditions are ANDed together. Each column is a column of the object type, and
    /// each value is text, a number, <c>true</c>, <c>false</c> or <c>null</c>.
    /// </summary>
    public Dictionary<string, JsonElement>? WhereEquals { get; set; }
}
