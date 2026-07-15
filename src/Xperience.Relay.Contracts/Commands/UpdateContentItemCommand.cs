using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Updates one or more fields on an existing reusable content item, preserving its current
/// published/draft state. Scalar fields go in <see cref="Fields"/>; linked-item fields (where
/// the value is a list of ContentItemGUIDs) go in <see cref="LinkedItemFields"/> — pass an
/// empty list to clear a field.
/// </summary>
[RelayCommand("update-content-item")]
public class UpdateContentItemCommand : IRelayCommand
{
    public int ContentItemId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>Scalar fields (string, int, bool, …). JSON element type is preserved by the handler.</summary>
    public Dictionary<string, JsonElement>? Fields { get; set; }

    /// <summary>
    /// Linked-items fields. Maps field name to a list of ContentItemGUIDs.
    /// An empty list clears the field.
    /// </summary>
    public Dictionary<string, List<Guid>>? LinkedItemFields { get; set; }

    /// <summary>
    /// Tag fields. Maps field name to a list of tag GUIDs.
    /// An empty list clears the field.
    /// </summary>
    public Dictionary<string, List<Guid>>? TagFields { get; set; }

    /// <summary>Optional binary assets to upload into asset fields. Each entry maps to one field.</summary>
    public List<RelayAsset>? Assets { get; set; }
}
