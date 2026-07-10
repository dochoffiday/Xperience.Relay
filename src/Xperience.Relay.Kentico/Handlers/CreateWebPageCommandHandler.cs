using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Creates a new web page under a parent web page item. The page is initially created as an
/// <c>InitialDraft</c>; set <see cref="CreateWebPageCommand.PublishAfterCreate"/> to true to
/// publish immediately. The website channel is resolved by channel code name via
/// <see cref="IInfoProvider{ChannelInfo}"/> → <see cref="IInfoProvider{WebsiteChannelInfo}"/>
/// so the command is channel-agnostic.
/// </summary>
public class CreateWebPageCommandHandler(
    IWebPageManagerFactory webPageManagerFactory,
    IInfoProvider<ChannelInfo> channelInfoProvider,
    IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<CreateWebPageCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(CreateWebPageCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ContentTypeName))
        {
            return RelayCommandResult.Fail("ContentTypeName must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            return RelayCommandResult.Fail("DisplayName must not be empty.");
        }

        var websiteChannelName = command.WebsiteChannelName ?? _options.DefaultWebsiteChannelName;
        if (string.IsNullOrWhiteSpace(websiteChannelName))
        {
            return RelayCommandResult.Fail("WebsiteChannelName is required. Set it on the command or configure RelayKenticoOptions.DefaultWebsiteChannelName.");
        }

        // Resolve channel by code name: ChannelInfo.ChannelName → WebsiteChannelInfo.WebsiteChannelChannelID
        var channel = channelInfoProvider.Get()
            .WhereEquals(nameof(ChannelInfo.ChannelName), websiteChannelName)
            .FirstOrDefault();

        if (channel is null)
        {
            return RelayCommandResult.Fail($"Channel '{websiteChannelName}' was not found.");
        }

        var websiteChannel = websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), channel.ChannelID)
            .FirstOrDefault();

        if (websiteChannel is null)
        {
            return RelayCommandResult.Fail($"Website channel for channel '{websiteChannelName}' was not found.");
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var fieldData = new Dictionary<string, object?>();

        if (command.Fields != null)
        {
            foreach (var (key, value) in command.Fields)
            {
                fieldData[key] = QueryItemsHelpers.DeserializeJsonElement(value);
            }
        }

        if (command.LinkedItemFields != null)
        {
            foreach (var (key, guids) in command.LinkedItemFields)
            {
                fieldData[key] = guids
                    .Select(g => new ContentItemReference { Identifier = g })
                    .ToList();
            }
        }

        if (command.TagFields != null)
        {
            foreach (var (key, guids) in command.TagFields)
            {
                fieldData[key] = new TagReferences
                {
                    Tags = guids.Select(g => new TagReference { Identifier = g }).ToList()
                };
            }
        }

        var userId = serviceAccountResolver.ResolveUserId();
        var webPageManager = webPageManagerFactory.Create(websiteChannel.WebsiteChannelID, userId);

        // Use the constructor that auto-generates the code name from the display name.
        var createParams = new CreateWebPageParameters(
            displayName: command.DisplayName,
            languageName: languageName,
            contentItemParameters: new ContentItemParameters(command.ContentTypeName, new ContentItemData(fieldData)))
        {
            ParentWebPageItemID = command.ParentWebPageItemId,
            VersionStatus = VersionStatus.InitialDraft,
            UrlSlug = command.UrlSlug,
        };

        var webPageItemId = await webPageManager.Create(createParams, cancellationToken);

        if (command.PublishAfterCreate)
        {
            await webPageManager.TryPublish(webPageItemId, languageName, cancellationToken);
        }

        var webPageMetadata = await webPageManager.GetWebPageMetadata(webPageItemId);

        return RelayCommandResult.Ok(
            message: $"Created web page '{command.DisplayName}' (ID={webPageItemId}){(command.PublishAfterCreate ? ", published" : ", draft")}.",
            data: new CreateWebPageResult
            {
                WebPageItemId = webPageItemId,
                WebPageItemGuid = webPageMetadata.WebPageGUID,
            });
    }

}
