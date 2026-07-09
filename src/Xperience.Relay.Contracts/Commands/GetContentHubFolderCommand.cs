namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Looks up a content hub folder by ID, codename, or path. When using <see cref="FolderPath"/>
/// the command is idempotent — it creates any missing path segments along the way. Exactly one
/// of <see cref="ContentFolderId"/>, <see cref="CodeName"/>, or <see cref="FolderPath"/> must
/// be supplied. Returns a <see cref="GetContentHubFolderResult"/> in
/// <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-content-hub-folder")]
public class GetContentHubFolderCommand : IRelayCommand
{
    /// <summary>Look up by numeric ID. Mutually exclusive with <see cref="CodeName"/> and <see cref="FolderPath"/>.</summary>
    public int? ContentFolderId { get; set; }

    /// <summary>Look up by code name. Mutually exclusive with <see cref="ContentFolderId"/> and <see cref="FolderPath"/>.</summary>
    public string? CodeName { get; set; }

    /// <summary>
    /// Slash-separated path, e.g. "Imports/Audio". All missing segments are created.
    /// Mutually exclusive with <see cref="ContentFolderId"/> and <see cref="CodeName"/>.
    /// </summary>
    public string? FolderPath { get; set; }

    /// <summary>Required when using <see cref="FolderPath"/>. Defaults to <c>RelayKenticoOptions.DefaultWorkspaceName</c> when null.</summary>
    public string? WorkspaceName { get; set; }
}
