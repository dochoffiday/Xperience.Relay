namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("update-tag", Access = RelayAccess.Write)]
public class UpdateTagCommand : IRelayCommand
{
    public int TagId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
