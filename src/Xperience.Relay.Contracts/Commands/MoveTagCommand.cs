namespace Xperience.Relay.Contracts.Commands;

/// <summary>Moves a tag under another tag, or to the root of its taxonomy, at a given position.</summary>
[RelayCommand("move-tag", Access = RelayAccess.Write)]
public class MoveTagCommand : IRelayCommand
{
    /// <summary>ID of the tag to move. Required.</summary>
    public int TagId { get; set; }

    /// <summary>ID of the new parent tag. Required; use 0 to move to the root of the taxonomy.</summary>
    public int TargetParentTagId { get; set; }

    /// <summary>Position among the new siblings. Required.</summary>
    public int Order { get; set; }
}
