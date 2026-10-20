namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("delete-contacts", Access = RelayAccess.Admin)]
public class DeleteContactsCommand : IRelayCommand
{
    public string WhereClause { get; set; } = string.Empty;
    public int? BatchSize { get; set; }
}
