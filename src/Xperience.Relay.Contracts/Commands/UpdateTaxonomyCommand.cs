namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("update-taxonomy")]
public class UpdateTaxonomyCommand : IRelayCommand
{
    public int TaxonomyId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
