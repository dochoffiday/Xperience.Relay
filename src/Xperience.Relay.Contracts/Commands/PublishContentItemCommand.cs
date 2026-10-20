namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Publishes a reusable content item language variant: a draft is published, and an unpublished item gets a draft
/// made first. Succeeds without changing anything if the item is already published.
/// </summary>
[RelayCommand("publish-content-item", Access = RelayAccess.Write)]
public class PublishContentItemCommand : IRelayCommand
{
    /// <summary>ID of the reusable content item. Required.</summary>
    public int ContentItemId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
