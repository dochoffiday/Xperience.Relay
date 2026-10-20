using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

public class PublishWebPageCommandHandler(
    IWebPageManagerFactory webPageManagerFactory,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
    ContentVersionReader versionReader,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<PublishWebPageCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(PublishWebPageCommand command, CancellationToken cancellationToken = default)
    {
        var webPageItem = webPageItemInfoProvider.Get()
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), command.WebPageId)
            .FirstOrDefault();

        if (webPageItem is null)
        {
            return RelayCommandResult.Fail($"Web page {command.WebPageId} was not found.");
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;
        var userId = serviceAccountResolver.ResolveUserId();
        var webPageManager = webPageManagerFactory.Create(webPageItem.WebPageItemWebsiteChannelID, userId);

        return await PublishingFlow.PublishAsync(
            $"web page {command.WebPageId} ({languageName})",
            versionReader.Read(webPageItem.WebPageItemContentItemID, languageName),
            () => webPageManager.TryCreateDraft(command.WebPageId, languageName, cancellationToken),
            () => webPageManager.TryPublish(command.WebPageId, languageName, cancellationToken));
    }
}
