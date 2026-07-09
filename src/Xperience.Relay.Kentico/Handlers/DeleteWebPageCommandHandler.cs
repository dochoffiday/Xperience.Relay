using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Deletes a language variant of a web page. Resolves the website channel from the page's own
/// row so the command arrives channel-agnostic (only a WebPageId is required).
/// </summary>
public class DeleteWebPageCommandHandler(
    IWebPageManagerFactory webPageManagerFactory,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<DeleteWebPageCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(DeleteWebPageCommand command, CancellationToken cancellationToken = default)
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

        var deleteParams = new DeleteWebPageParameters(command.WebPageId, languageName)
        {
            Permanently = command.Permanently,
        };

        if (command.RedirectToWebPageId.HasValue)
        {
            deleteParams.RedirectToWebPageID = command.RedirectToWebPageId.Value;
        }

        await webPageManager.Delete(deleteParams, cancellationToken);

        return RelayCommandResult.Ok($"Deleted web page {command.WebPageId} ({languageName}){(command.Permanently ? " permanently" : "")}.");
    }
}
