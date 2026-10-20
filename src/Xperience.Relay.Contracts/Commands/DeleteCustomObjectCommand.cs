namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Deletes an object of any registered Kentico object type by ID, through Kentico so its events and cache
/// invalidation run as usual. The relay's own API key object type is refused.
/// </summary>
[RelayCommand("delete-custom-object", Access = RelayAccess.Admin)]
public class DeleteCustomObjectCommand : IRelayCommand
{
    /// <summary>Registered object type name, e.g. <c>custom.redirect</c>. Required.</summary>
    public string ObjectTypeName { get; set; } = string.Empty;

    /// <summary>ID of the object to delete. Required; must be positive.</summary>
    public int Id { get; set; }
}
