namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Publishes a reusable content item language variant. No-ops if the item is already published.
/// </summary>
[RelayCommand("publish-content-item")]
public class PublishContentItemCommand : IRelayCommand
{
    public int ContentItemId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
