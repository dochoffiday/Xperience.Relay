namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("delete-custom-object")]
public class DeleteCustomObjectCommand : IRelayCommand
{
    public string ObjectTypeName { get; set; } = string.Empty;
    public int Id { get; set; }
}
