using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Moves a web page under a new parent, appending it as the last child. The target parent's
/// website channel isn't known up front, so it's resolved from the moved page itself via a
/// direct WebPageItemInfo lookup before a channel-scoped <see cref="IWebPageManager"/> can
/// be created.
/// </summary>
public class MoveWebPageCommandHandler(
    IWebPageManagerFactory webPageManagerFactory,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
    ServiceAccountResolver serviceAccountResolver) : IRelayCommandHandler<MoveWebPageCommand>
{
    public async Task<RelayCommandResult> HandleAsync(MoveWebPageCommand command, CancellationToken cancellationToken = default)
    {
        var webPageItem = webPageItemInfoProvider.Get()
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), command.WebPageId)
            .FirstOrDefault();

        if (webPageItem is null)
        {
            return RelayCommandResult.Fail($"Web page {command.WebPageId} was not found.");
        }

        var userId = serviceAccountResolver.ResolveUserId();
        var webPageManager = webPageManagerFactory.Create(webPageItem.WebPageItemWebsiteChannelID, userId);

        await webPageManager.Move(
            new MoveWebPageParameters(command.WebPageId, command.ParentWebPageId),
            cancellationToken);

        return RelayCommandResult.Ok($"Moved web page {command.WebPageId} under parent {command.ParentWebPageId}.");
    }
}
