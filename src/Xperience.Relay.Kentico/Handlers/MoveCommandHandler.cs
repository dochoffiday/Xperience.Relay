using CMS.ContentEngine;
using CMS.Websites;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Moves a web page under a new parent, appending it as the last child. The target parent's
/// website channel isn't known up front, so it's resolved from the moved page itself via a
/// channel-agnostic content item query before a channel-scoped <see cref="IWebPageManager"/> can
/// be created.
/// </summary>
public class MoveCommandHandler : IRelayCommandHandler<MoveCommand>
{
    private readonly IWebPageManagerFactory _webPageManagerFactory;
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly ServiceAccountResolver _serviceAccountResolver;

    public MoveCommandHandler(
        IWebPageManagerFactory webPageManagerFactory,
        IContentQueryExecutor contentQueryExecutor,
        ServiceAccountResolver serviceAccountResolver)
    {
        _webPageManagerFactory = webPageManagerFactory;
        _contentQueryExecutor = contentQueryExecutor;
        _serviceAccountResolver = serviceAccountResolver;
    }

    public async Task<RelayCommandResult> HandleAsync(MoveCommand command, CancellationToken cancellationToken = default)
    {
        var channelId = await GetWebsiteChannelIdAsync(command.WebPageId, cancellationToken);
        if (channelId is null)
        {
            return RelayCommandResult.Fail($"Web page {command.WebPageId} was not found.");
        }

        var userId = _serviceAccountResolver.ResolveUserId();
        var webPageManager = _webPageManagerFactory.Create(channelId.Value, userId);

        await webPageManager.Move(
            new MoveWebPageParameters(command.WebPageId, command.ParentWebPageId),
            cancellationToken);

        return RelayCommandResult.Ok($"Moved web page {command.WebPageId} under parent {command.ParentWebPageId}.");
    }

    private async Task<int?> GetWebsiteChannelIdAsync(int webPageId, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(_ => { })
            .Parameters(p => p.Where(w => w.WhereEquals("WebPageItemID", webPageId)));

        var channelIds = await _contentQueryExecutor.GetWebPageResult(
            builder,
            container => (int?)container.WebPageItemWebsiteChannelID,
            cancellationToken: cancellationToken);

        return channelIds.FirstOrDefault();
    }
}
