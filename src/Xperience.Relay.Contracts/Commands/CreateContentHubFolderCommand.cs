namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Ensures a content hub folder path exists, creating any missing segments along the way.
/// Idempotent — safe to call even if the path already exists.
/// Returns a <see cref="CreateContentHubFolderResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("create-content-hub-folder")]
public class CreateContentHubFolderCommand : IRelayCommand
{
    /// <summary>Slash-separated path, e.g. "Imports/Audio". All missing segments are created.</summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultWorkspaceName</c> when null.</summary>
    public string? WorkspaceName { get; set; }
}
