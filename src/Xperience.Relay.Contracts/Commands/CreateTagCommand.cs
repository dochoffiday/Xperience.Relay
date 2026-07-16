namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("create-tag")]
public class CreateTagCommand : IRelayCommand
{
    /// <summary>Identify the taxonomy by ID or code name — one is required.</summary>
    public int? TaxonomyId { get; set; }
    public string? TaxonomyName { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Optional code name. Auto-generated from <see cref="Title"/> when omitted.</summary>
    public string? CodeName { get; set; }

    /// <summary>ID of the parent tag. Omit or set to 0 for a root-level tag.</summary>
    public int? ParentTagId { get; set; }
    public int? Order { get; set; }
}
