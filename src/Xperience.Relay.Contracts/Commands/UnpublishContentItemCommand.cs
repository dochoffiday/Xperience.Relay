namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Unpublishes a reusable content item language variant. Succeeds without changing anything if the item is already
/// unpublished or has only a draft. Fails if the item is published and also has a draft with unpublished changes,
/// because Kentico won't unpublish it until that draft is published or discarded.
/// </summary>
[RelayCommand("unpublish-content-item", Access = RelayAccess.Write)]
public class UnpublishContentItemCommand : IRelayCommand
{
    /// <summary>ID of the reusable content item. Required.</summary>
    public int ContentItemId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
