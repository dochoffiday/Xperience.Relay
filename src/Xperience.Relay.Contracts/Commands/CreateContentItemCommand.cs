using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Creates a new reusable content item, optionally uploading a binary asset into one of its fields.
/// The asset is passed as a Base64-encoded string so the command travels as plain JSON. On the
/// server side the handler decodes it to a temp file, builds the Kentico asset metadata, and
/// cleans up when done. Returns a <see cref="CreateContentItemResult"/> in
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

    /// <summary>Optional binary asset to upload into one of the content item's asset fields.</summary>
    public RelayAsset? Asset { get; set; }
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
