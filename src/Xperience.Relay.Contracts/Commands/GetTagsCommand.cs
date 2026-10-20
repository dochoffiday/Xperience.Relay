namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Returns tags matching the given filter. At least one filter must be provided.
/// <see cref="TaxonomyId"/> or <see cref="TaxonomyName"/> returns all tags for a taxonomy.
/// <see cref="TagId"/> or <see cref="TagName"/> returns the matching tag(s).
/// When several filters are given only one is used, in the order TagId, TagName, TaxonomyId, TaxonomyName.
/// Returns a <see cref="GetTagsResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-tags", Access = RelayAccess.Read)]
public class GetTagsCommand : IRelayCommand
{
    /// <summary>All tags of the taxonomy with this ID.</summary>
    public int? TaxonomyId { get; set; }

    /// <summary>All tags of the taxonomy with this code name.</summary>
    public string? TaxonomyName { get; set; }

    /// <summary>The tag with this ID.</summary>
    public int? TagId { get; set; }

    /// <summary>The tag(s) with this code name.</summary>
    public string? TagName { get; set; }
}
