namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("create-taxonomy")]
public class CreateTaxonomyCommand : IRelayCommand
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Optional code name. Auto-generated from <see cref="Title"/> when omitted.</summary>
    public string? CodeName { get; set; }
}
