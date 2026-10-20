namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Finds the content items that directly reference a content item's language variant (the "used by" list in the admin
/// UI). Returns a <see cref="GetContentItemUsageResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-content-item-usage", Access = RelayAccess.Read)]
public class GetContentItemUsageCommand : IRelayCommand
{
    /// <summary>ID of the content item (reusable item or page content item) to look up. Required; must be positive.</summary>
    public int ContentItemId { get; set; }

    /// <summary>Language variant to look up, e.g. <c>en</c>. Required; there is no default.</summary>
    public string LanguageName { get; set; } = string.Empty;
}
