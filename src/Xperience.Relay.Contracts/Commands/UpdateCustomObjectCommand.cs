using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Updates an object of any registered Kentico object type by ID, changing only the given columns, through Kentico so
/// its events and cache invalidation run as usual. The relay's own API key object type is refused.
/// </summary>
[RelayCommand("update-custom-object", Access = RelayAccess.Admin)]
public class UpdateCustomObjectCommand : IRelayCommand
{
    /// <summary>Registered object type name, e.g. <c>custom.redirect</c>. Required.</summary>
    public string ObjectTypeName { get; set; } = string.Empty;

    /// <summary>ID of the object to update. Required; must be positive.</summary>
    public int Id { get; set; }

    /// <summary>Column name to new value. Only these columns change. At least one is required.</summary>
    public Dictionary<string, JsonElement> Fields { get; set; } = [];
}
