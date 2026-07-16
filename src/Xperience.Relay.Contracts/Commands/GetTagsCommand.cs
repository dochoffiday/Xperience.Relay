namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Returns tags matching the given filter. At least one filter must be provided.
/// <see cref="TaxonomyId"/> or <see cref="TaxonomyName"/> returns all tags for a taxonomy.
/// <see cref="TagId"/> or <see cref="TagName"/> returns the matching tag(s).
/// Results are in <see cref="GetTagsResult"/>.
/// </summary>
[RelayCommand("get-tags")]
public class GetTagsCommand : IRelayCommand
{
    public int? TaxonomyId { get; set; }
    public string? TaxonomyName { get; set; }
    public int? TagId { get; set; }
    public string? TagName { get; set; }
}
