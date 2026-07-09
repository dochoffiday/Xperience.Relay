namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Unpublishes a reusable content item language variant. No-ops if the item is already unpublished.
/// </summary>
[RelayCommand("unpublish-content-item")]
public class UnpublishContentItemCommand : IRelayCommand
{
    public int ContentItemId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
