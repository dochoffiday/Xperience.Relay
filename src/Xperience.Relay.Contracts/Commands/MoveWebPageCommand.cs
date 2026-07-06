namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Moves an existing web page under a different parent. Assumes <see cref="ParentWebPageId"/>
/// already exists -- this command does not create folders along the way, and the caller is
/// expected to have already resolved the target parent's ID (e.g. via "get-page-info"). Does not
/// specify an explicit sibling order; the moved page is appended as the last child. Use a separate
/// sort command afterward if a specific order is required.
/// </summary>
[RelayCommand("move-web-page")]
public class MoveWebPageCommand : IRelayCommand
{
    public int WebPageId { get; set; }
    public int ParentWebPageId { get; set; }
}
