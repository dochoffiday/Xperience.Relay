using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Creates an object of any registered Kentico object type (typically a custom module class) by setting the given
/// columns and inserting it, so Kentico's events and cache invalidation run as usual. The relay's own API key
/// object type is refused. Returns a <see cref="CreateCustomObjectResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("create-custom-object", Access = RelayAccess.Admin)]
public class CreateCustomObjectCommand : IRelayCommand
{
    /// <summary>Registered object type name, e.g. <c>custom.redirect</c>. Required.</summary>
    public string ObjectTypeName { get; set; } = string.Empty;

    /// <summary>Column name to value for the new object. Required.</summary>
    public Dictionary<string, JsonElement> Fields { get; set; } = [];
}
