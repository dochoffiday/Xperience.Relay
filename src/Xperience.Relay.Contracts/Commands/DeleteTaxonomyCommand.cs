namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("delete-taxonomy")]
public class DeleteTaxonomyCommand : IRelayCommand
{
    public int TaxonomyId { get; set; }
}
