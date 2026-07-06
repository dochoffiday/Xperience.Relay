namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "get-content-hub-folder" command.
/// </summary>
public class GetContentHubFolderResult
{
    public int ContentFolderId { get; set; }
}
