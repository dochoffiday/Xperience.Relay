namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Moves an existing web page under a different parent, or to the root of its channel. The parent has to exist already:
/// the command doesn't create pages along the way, so the caller resolves the target parent's ID first (e.g. with
/// "get-page-info"). The command doesn't set a sibling order -- Kentico puts the moved page first among its new
/// siblings -- and the parent's content type must allow the page's content type as a child.
/// </summary>
[RelayCommand("move-web-page", Access = RelayAccess.Write)]
public class MoveWebPageCommand : IRelayCommand
{
    /// <summary>ID of the page to move. Required.</summary>
    public int WebPageId { get; set; }

    /// <summary>ID of the new parent page, or 0 for the root of the channel. Required.</summary>
    public int ParentWebPageId { get; set; }
}
