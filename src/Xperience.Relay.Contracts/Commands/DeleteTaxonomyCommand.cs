namespace Xperience.Relay.Contracts.Commands;

/// <summary>Deletes a taxonomy. All of its tags are deleted with it.</summary>
[RelayCommand("delete-taxonomy", Access = RelayAccess.Write)]
public class DeleteTaxonomyCommand : IRelayCommand
{
    /// <summary>ID of the taxonomy to delete. Required.</summary>
    public int TaxonomyId { get; set; }
}
