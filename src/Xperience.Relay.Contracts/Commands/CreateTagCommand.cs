namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Creates a tag in a taxonomy, at the root or under another tag. Returns a <see cref="CreateTagResult"/> in
/// <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("create-tag", Access = RelayAccess.Write)]
public class CreateTagCommand : IRelayCommand
{
    /// <summary>ID of the taxonomy to add the tag to. Give this or <see cref="TaxonomyName"/>; one is required.</summary>
    public int? TaxonomyId { get; set; }

    /// <summary>Code name of the taxonomy to add the tag to. Give this or <see cref="TaxonomyId"/>; one is required.</summary>
    public string? TaxonomyName { get; set; }

    /// <summary>Display title of the tag. Required.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional code name. Made from <see cref="Title"/> when omitted: lower case, with each run of characters other than
    /// letters and digits replaced by a hyphen (<c>Early spring</c> becomes <c>early-spring</c>).
    /// </summary>
    public string? CodeName { get; set; }

    /// <summary>ID of the parent tag. Omit or set to 0 for a root-level tag.</summary>
    public int? ParentTagId { get; set; }

    /// <summary>Position among the tag's siblings. When omitted the order isn't set, and the tag reads back with order 0.</summary>
    public int? Order { get; set; }
}
