using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Creates a new reusable content item, optionally uploading one or more binary assets into asset
/// fields. Each asset is passed as a Base64-encoded string so the command travels as plain JSON. On
/// the server side each entry is decoded to a temp file, wrapped in Kentico asset metadata, and
/// cleaned up in a finally block regardless of outcome. Returns a <see cref="CreateContentItemResult"/> in
/// <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("create-content-item")]
public class CreateContentItemCommand : IRelayCommand
{
    public string ContentTypeName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultWorkspaceName</c> when null.</summary>
    public string? WorkspaceName { get; set; }

    /// <summary>
    /// Content hub folder to place the item in after creation. Use the "get-content-hub-folder"
    /// command first to resolve or create the target folder and get its ID.
    /// </summary>
    public int? ContentFolderId { get; set; }

    /// <summary>Scalar fields (string, int, bool, …). JSON element type is preserved by the handler.</summary>
    public Dictionary<string, JsonElement>? Fields { get; set; }

    /// <summary>
    /// Linked-items fields. Maps field name to a list of ContentItemGUIDs.
    /// </summary>
    public Dictionary<string, List<Guid>>? LinkedItemFields { get; set; }

    /// <summary>
    /// Tag fields. Maps field name to a list of tag GUIDs.
    /// </summary>
    public Dictionary<string, List<Guid>>? TagFields { get; set; }

    /// <summary>Optional binary assets to upload into asset fields. Each entry maps to one field.</summary>
    public List<RelayAsset>? Assets { get; set; }
}

/// <summary>
/// A binary file to be uploaded as part of a <see cref="CreateContentItemCommand"/>.
/// </summary>
public class RelayAsset
{
    /// <summary>Name of the content item field to populate, e.g. "FileAsset".</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>File name including extension, e.g. "workshop-audio.mp3". Drives the asset metadata name and extension.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Base64-encoded bytes of the file.</summary>
    public string Base64 { get; set; } = string.Empty;

    public bool IsValid() =>
        !string.IsNullOrEmpty(FieldName) &&
        !string.IsNullOrEmpty(FileName) &&
        !string.IsNullOrEmpty(Base64);
}
