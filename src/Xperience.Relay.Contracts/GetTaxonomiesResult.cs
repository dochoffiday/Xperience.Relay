namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "get-taxonomies" command.</summary>
public class GetTaxonomiesResult
{
    /// <summary>All taxonomies.</summary>
    public List<TaxonomyResult> Taxonomies { get; set; } = [];
}

/// <summary>A taxonomy, as listed by "get-taxonomies".</summary>
public class TaxonomyResult
{
    /// <summary>ID of the taxonomy.</summary>
    public int TaxonomyId { get; set; }

    /// <summary>GUID of the taxonomy.</summary>
    public Guid TaxonomyGuid { get; set; }

    /// <summary>The taxonomy's code name.</summary>
    public string TaxonomyName { get; set; } = string.Empty;

    /// <summary>Display title of the taxonomy.</summary>
    public string TaxonomyTitle { get; set; } = string.Empty;

    /// <summary>Description of the taxonomy; empty when it has none.</summary>
    public string? TaxonomyDescription { get; set; }
}
