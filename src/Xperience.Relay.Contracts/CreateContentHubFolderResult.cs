namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "create-content-hub-folder" command.
/// </summary>
public class CreateContentHubFolderResult
{
    public int ContentFolderId { get; set; }
}
