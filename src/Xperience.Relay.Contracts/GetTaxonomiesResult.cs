namespace Xperience.Relay.Contracts;

public class GetTaxonomiesResult
{
    public List<TaxonomyResult> Taxonomies { get; set; } = [];
}

public class TaxonomyResult
{
    public int TaxonomyId { get; set; }
    public Guid TaxonomyGuid { get; set; }
    public string TaxonomyName { get; set; } = string.Empty;
    public string TaxonomyTitle { get; set; } = string.Empty;
    public string? TaxonomyDescription { get; set; }
}
