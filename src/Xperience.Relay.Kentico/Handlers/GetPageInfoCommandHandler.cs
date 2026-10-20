using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
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
    IInfoProvider<ChannelInfo> channelInfoProvider,
    IInfoProvider<ContentItemInfo> contentItemInfoProvider,
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
            ContentType = ResolveContentTypeName(webPageItem.WebPageItemContentItemID),
            LanguageName = languageName,
            ChannelName = ResolveChannelName(webPageItem.WebPageItemWebsiteChannelID),
        };
    }

    // A page is a content item; it's the item's content type ID (not the item's own ID) that names its class.
    private string ResolveContentTypeName(int contentItemId)
    {
        var contentItem = contentItemInfoProvider.Get()
            .WhereEquals(nameof(ContentItemInfo.ContentItemID), contentItemId)
            .FirstOrDefault();

        return contentItem is null ? string.Empty : DataClassInfoProvider.GetClassName(contentItem.ContentItemContentTypeID) ?? string.Empty;
    }

    // The channel's code name, e.g. "MyChannel" -- not the website channel's domain.
    private string ResolveChannelName(int websiteChannelId)
    {
        var websiteChannel = websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), websiteChannelId)
            .FirstOrDefault();

        if (websiteChannel is null)
        {
            return string.Empty;
        }

        return channelInfoProvider.Get()
            .WhereEquals(nameof(ChannelInfo.ChannelID), websiteChannel.WebsiteChannelChannelID)
            .FirstOrDefault()?.ChannelName ?? string.Empty;
    }
}
