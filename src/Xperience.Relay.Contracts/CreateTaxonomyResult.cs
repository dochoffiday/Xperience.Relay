namespace Xperience.Relay.Contracts;

public class CreateTaxonomyResult
{
    public int TaxonomyId { get; set; }
    public Guid TaxonomyGuid { get; set; }
    public string TaxonomyName { get; set; } = string.Empty;
    public string TaxonomyTitle { get; set; } = string.Empty;
}
