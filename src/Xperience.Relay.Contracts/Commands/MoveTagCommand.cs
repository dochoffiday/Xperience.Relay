namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("move-tag")]
public class MoveTagCommand : IRelayCommand
{
    public int TagId { get; set; }

    /// <summary>ID of the new parent tag. Use 0 to move to the root of the taxonomy.</summary>
    public int TargetParentTagId { get; set; }
    public int Order { get; set; }
}
