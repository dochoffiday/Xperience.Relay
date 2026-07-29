namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("delete-contacts")]
public class DeleteContactsCommand : IRelayCommand
{
    public string WhereClause { get; set; } = string.Empty;
    public int? BatchSize { get; set; }
}
