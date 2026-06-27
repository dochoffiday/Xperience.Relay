using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class GetPageInfoCommandHandler : IRelayCommandHandler<GetPageInfoCommand>
{
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;
    private readonly RelayKenticoOptions _options;

    public GetPageInfoCommandHandler(
        IContentQueryExecutor contentQueryExecutor,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IOptions<RelayKenticoOptions> options)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _websiteChannelInfoProvider = websiteChannelInfoProvider;
        _options = options.Value;
    }

    public async Task<RelayCommandResult> HandleAsync(GetPageInfoCommand command, CancellationToken cancellationToken = default)
    {
        var info = await FetchWebPageInfoAsync(command.WebPageId, command.LanguageName ?? _options.DefaultLanguageName, cancellationToken);
        return info is null
            ? RelayCommandResult.Fail($"Web page {command.WebPageId} was not found.")
            : RelayCommandResult.Ok(data: info);
    }

    internal async Task<WebPageInfo?> FetchWebPageInfoAsync(int webPageId, string languageName, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(_ => { })
            .InLanguage(languageName)
            .Parameters(p => p.Where(w => w.WhereEquals("WebPageItemID", webPageId)));

        var rows = await _contentQueryExecutor.GetWebPageResult(
            builder,
            container => new WebPageInfo
            {
                WebPageId = container.WebPageItemID,
                WebPageGuid = container.WebPageItemGUID,
                Name = container.WebPageItemName,
                TreePath = container.WebPageItemTreePath,
                ParentWebPageId = container.WebPageItemParentID == 0 ? null : container.WebPageItemParentID,
                ContentType = DataClassInfoProvider.GetClassName(container.ContentItemContentTypeID),
                LanguageName = languageName,
                ChannelName = ResolveChannelName(container.WebPageItemWebsiteChannelID),
            },
            cancellationToken: cancellationToken);

        return rows.FirstOrDefault();
    }

    private string ResolveChannelName(int channelId) =>
        _websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), channelId)
            .FirstOrDefault()?.WebsiteChannelDomain ?? string.Empty;
}
