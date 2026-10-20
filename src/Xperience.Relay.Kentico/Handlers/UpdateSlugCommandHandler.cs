using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using CMS.Websites.Routing;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Updates the URL slug on a web page draft. If the page was published, the updated draft
/// is immediately re-published so the slug change goes live.
/// </summary>
public class UpdateSlugCommandHandler(
    IWebPageUrlPathDataRetriever webPageUrlPathDataRetriever,
    IWebPageManagerFactory webPageManagerFactory,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
    ContentVersionReader versionReader,
    FailedWriteCacheCleaner cacheCleaner,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<UpdateSlugCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(UpdateSlugCommand command, CancellationToken cancellationToken = default)
    {
        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var webPageItem = webPageItemInfoProvider.Get()
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), command.WebPageId)
            .FirstOrDefault();

        if (webPageItem is null)
        {
            return RelayCommandResult.Fail($"Web page {command.WebPageId} was not found.");
        }

        var subject = $"web page {command.WebPageId} ({languageName})";
        var state = versionReader.Read(webPageItem.WebPageItemContentItemID, languageName);

        if (state is null)
        {
            return DraftEditFlow.NotFound(subject);
        }

        var userId = serviceAccountResolver.ResolveUserId();
        var webPageManager = webPageManagerFactory.Create(webPageItem.WebPageItemWebsiteChannelID, userId);

        return await DraftEditFlow.EditAsync(
            subject,
            state,
            new DraftOperations(
                CreateDraft: () => webPageManager.TryCreateDraft(command.WebPageId, languageName, cancellationToken),
                SaveDraft: async () =>
                {
                    var urlPathData = await webPageUrlPathDataRetriever.GetDraftData(
                        command.WebPageId,
                        languageName,
                        cancellationToken);

                    if (urlPathData is null)
                    {
                        return $"Could not retrieve draft URL data for web page {command.WebPageId} in language '{languageName}'.";
                    }

                    urlPathData.EditSystemUrlSlug(command.Slug);

                    return DraftEditFlow.SavedOrError(
                        await webPageManager.TryUpdateDraft(command.WebPageId, languageName, new UpdateDraftData(urlPathData), cancellationToken),
                        subject);
                },
                Publish: () => webPageManager.TryPublish(command.WebPageId, languageName, cancellationToken),
                DiscardDraft: () => webPageManager.TryDiscardDraft(command.WebPageId, languageName, cancellationToken),
                ForgetCachedCopies: () => cacheCleaner.Forget(webPageItem.WebPageItemContentItemID, languageName)),
            $"Updated slug on web page {command.WebPageId} to '{command.Slug}'.");
    }
}
