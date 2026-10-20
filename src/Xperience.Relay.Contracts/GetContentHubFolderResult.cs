namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "get-content-hub-folder" command.
/// </summary>
public class GetContentHubFolderResult
{
    /// <summary>ID of the folder that was found or created, for "create-content-item" and "move-content-item".</summary>
    public int ContentFolderId { get; set; }
}
