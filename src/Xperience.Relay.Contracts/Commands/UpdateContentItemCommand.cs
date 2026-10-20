using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Updates one or more fields on an existing reusable content item. The item is edited through a draft, and
/// published again only if it was published before; a draft or an unpublished item stays unpublished. Plain values go
/// in <see cref="Fields"/>; linked-item fields (where the value is a list of ContentItemGUIDs) go in
/// <see cref="LinkedItemFields"/> -- pass an empty list to clear a field. Every field is checked against the content
/// type before anything changes.
/// </summary>
[RelayCommand("update-content-item", Access = RelayAccess.Write)]
public class UpdateContentItemCommand : IRelayCommand
{
    /// <summary>ID of the reusable content item. Required.</summary>
    public int ContentItemId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>Plain field values by field name (text, numbers, booleans, dates, ...).</summary>
    public Dictionary<string, JsonElement>? Fields { get; set; }

    /// <summary>
    /// Linked-items fields. Maps field name to a list of ContentItemGUIDs. The field must be a content item
    /// reference field. An empty list clears the field.
    /// </summary>
    public Dictionary<string, List<Guid>>? LinkedItemFields { get; set; }

    /// <summary>
    /// Tag fields. Maps field name to a list of tag GUIDs. The field must be a tag (taxonomy) field.
    /// An empty list clears the field.
    /// </summary>
    public Dictionary<string, List<Guid>>? TagFields { get; set; }

    /// <summary>Optional binary assets to upload into asset fields. Each entry maps to one field.</summary>
    public List<RelayAsset>? Assets { get; set; }
}
