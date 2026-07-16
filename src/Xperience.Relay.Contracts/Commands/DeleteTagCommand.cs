namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("delete-tag")]
public class DeleteTagCommand : IRelayCommand
{
    public int TagId { get; set; }
}
