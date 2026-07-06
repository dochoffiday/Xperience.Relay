namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Moves one or more reusable content items into a content hub folder. Use "get-content-hub-folder"
/// first to resolve or create the target folder and obtain its ID.
/// </summary>
[RelayCommand("move-content-item")]
public class MoveContentItemCommand : IRelayCommand
{
    /// <summary>IDs of the content items to move.</summary>
    public List<int> ContentItemIds { get; set; } = [];

    /// <summary>ID of the destination content hub folder.</summary>
    public int ContentFolderId { get; set; }
}
