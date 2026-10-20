namespace Xperience.Relay.Contracts.Commands;

/// <summary>Deletes a tag. Its child tags are deleted with it.</summary>
[RelayCommand("delete-tag", Access = RelayAccess.Write)]
public class DeleteTagCommand : IRelayCommand
{
    /// <summary>ID of the tag to delete. Required.</summary>
    public int TagId { get; set; }
}
