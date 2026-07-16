namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("update-tag")]
public class UpdateTagCommand : IRelayCommand
{
    public int TagId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
