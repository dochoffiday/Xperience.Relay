namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("delete-taxonomy", Access = RelayAccess.Write)]
public class DeleteTaxonomyCommand : IRelayCommand
{
    public int TaxonomyId { get; set; }
}
