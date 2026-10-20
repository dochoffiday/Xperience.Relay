namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "create-taxonomy" command.</summary>
public class CreateTaxonomyResult
{
    /// <summary>ID of the new taxonomy.</summary>
    public int TaxonomyId { get; set; }

    /// <summary>GUID of the new taxonomy.</summary>
    public Guid TaxonomyGuid { get; set; }

    /// <summary>The taxonomy's code name.</summary>
    public string TaxonomyName { get; set; } = string.Empty;

    /// <summary>Display title of the new taxonomy.</summary>
    public string TaxonomyTitle { get; set; } = string.Empty;
}
