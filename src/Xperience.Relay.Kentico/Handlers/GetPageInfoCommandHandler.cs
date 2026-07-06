using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class GetPageInfoCommandHandler(
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
    IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<GetPageInfoCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public Task<RelayCommandResult> HandleAsync(GetPageInfoCommand command, CancellationToken cancellationToken = default)
    {
        var languageName = command.LanguageName ?? _options.DefaultLanguageName;
        var info = FetchWebPageInfo(command.WebPageId, languageName);
        return Task.FromResult(info is null
            ? RelayCommandResult.Fail($"Web page {command.WebPageId} was not found.")
            : RelayCommandResult.Ok(data: info));
    }

    internal WebPageInfo? FetchWebPageInfo(int webPageId, string languageName)
    {
        var webPageItem = webPageItemInfoProvider.Get()
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), webPageId)
            .FirstOrDefault();

        if (webPageItem is null)
        {
            return null;
        }

        return new WebPageInfo
        {
            WebPageId = webPageItem.WebPageItemID,
            WebPageGuid = webPageItem.WebPageItemGUID,
            Name = webPageItem.WebPageItemName,
            TreePath = webPageItem.WebPageItemTreePath,
            ParentWebPageId = webPageItem.WebPageItemParentID == 0 ? null : webPageItem.WebPageItemParentID,
            ContentType = DataClassInfoProvider.GetClassName(webPageItem.WebPageItemContentItemID),
            LanguageName = languageName,
            ChannelName = ResolveChannelName(webPageItem.WebPageItemWebsiteChannelID),
        };
    }

    private string ResolveChannelName(int channelId) =>
        websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), channelId)
            .FirstOrDefault()?.WebsiteChannelDomain ?? string.Empty;
}
