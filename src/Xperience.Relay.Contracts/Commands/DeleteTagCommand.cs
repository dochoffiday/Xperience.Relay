namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("delete-tag", Access = RelayAccess.Write)]
public class DeleteTagCommand : IRelayCommand
{
    public int TagId { get; set; }
}
