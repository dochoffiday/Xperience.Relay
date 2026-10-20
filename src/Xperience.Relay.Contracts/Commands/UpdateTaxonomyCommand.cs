namespace Xperience.Relay.Contracts.Commands;

/// <summary>Changes a taxonomy's title and/or description. Give at least one of them.</summary>
[RelayCommand("update-taxonomy", Access = RelayAccess.Write)]
public class UpdateTaxonomyCommand : IRelayCommand
{
    /// <summary>ID of the taxonomy. Required.</summary>
    public int TaxonomyId { get; set; }

    /// <summary>New title. Left as it is when omitted.</summary>
    public string? Title { get; set; }

    /// <summary>New description. Left as it is when omitted; an empty string clears it.</summary>
    public string? Description { get; set; }
}
