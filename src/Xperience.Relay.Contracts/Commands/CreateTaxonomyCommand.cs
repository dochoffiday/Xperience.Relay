namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Creates a taxonomy. Returns a <see cref="CreateTaxonomyResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("create-taxonomy", Access = RelayAccess.Write)]
public class CreateTaxonomyCommand : IRelayCommand
{
    /// <summary>Display title of the taxonomy. Required.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional code name. Made from <see cref="Title"/> when omitted: lower case, with each run of characters other than
    /// letters and digits replaced by a hyphen (<c>Coffee tastes</c> becomes <c>coffee-tastes</c>).
    /// </summary>
    public string? CodeName { get; set; }
}
