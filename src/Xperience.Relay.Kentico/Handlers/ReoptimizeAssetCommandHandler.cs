using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Extensions;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Re-triggers Kentico's asset optimization pipeline for an existing asset field without
/// transferring any binary data over the wire. Looks up the current asset metadata and its
/// physical file path on the server, then re-submits the same file through
/// <see cref="ContentItemAssetMetadataWithSource"/> so Kentico processes it identically to
/// the initial upload.
/// </summary>
public class ReoptimizeAssetCommandHandler(
    IContentItemManagerFactory contentItemManagerFactory,
    IInfoProvider<ContentItemInfo> contentItemInfoProvider,
    IInfoProvider<ContentItemCommonDataInfo> contentItemCommonDataInfoProvider,
    IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
    IContentItemAssetMetadataItemProvider contentItemAssetMetadataItemProvider,
    IContentItemAssetPathProvider contentItemAssetPathProvider,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<ReoptimizeAssetCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(ReoptimizeAssetCommand command, CancellationToken cancellationToken = default)
    {
        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var contentItem = contentItemInfoProvider.Get()
            .WhereEquals(nameof(ContentItemInfo.ContentItemID), command.ContentItemId)
            .FirstOrDefault();

        if (contentItem is null)
        {
            return RelayCommandResult.Fail($"Content item {command.ContentItemId} was not found.");
        }

        var dataClass = DataClassInfoProvider
            .GetClasses()
            .WhereEquals(nameof(DataClassInfo.ClassID), contentItem.ContentItemContentTypeID)
            .FirstOrDefault();

        if (dataClass is null)
        {
            return RelayCommandResult.Fail($"Content type for content item {command.ContentItemId} was not found.");
        }

        var formInfo = new FormInfo(dataClass.ClassFormDefinition);
        var field = formInfo.GetFormField(command.FieldName);

        if (field is null)
        {
            return RelayCommandResult.Fail($"Field '{command.FieldName}' was not found on content type '{dataClass.ClassName}'.");
        }

        var language = contentLanguageInfoProvider.Get()
            .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), languageName)
            .FirstOrDefault();

        if (language is null)
        {
            return RelayCommandResult.Fail($"Language '{languageName}' was not found.");
        }

        var assetMetadata = await contentItemAssetMetadataItemProvider.Get(
            contentItemGuid: contentItem.ContentItemGUID,
            fieldGuid: field.Guid,
            contentLanguageId: language.ContentLanguageID,
            isLatest: true,
            isAdministration: true,
            cancellationToken: cancellationToken);

        if (assetMetadata is null || assetMetadata.Metadata is null)
        {
            return RelayCommandResult.Fail($"No asset found for field '{command.FieldName}' on content item {command.ContentItemId} in language '{languageName}'.");
        }

        var filePath = contentItemAssetPathProvider.GetFileLocation(
            assetMetadata.Metadata,
            contentItem.ContentItemGUID,
            field.Guid);

        var commonData = contentItemCommonDataInfoProvider.Get()
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID), command.ContentItemId)
            .WhereTrue(nameof(ContentItemCommonDataInfo.ContentItemCommonDataIsLatest))
            .FirstOrDefault();

        var isPublished = commonData.IsPublished();

        var fieldData = new Dictionary<string, object?>
        {
            [command.FieldName] = new ContentItemAssetMetadataWithSource(
                new ContentItemAssetFileSource(filePath, false),
                assetMetadata.Metadata)
        };

        var userId = serviceAccountResolver.ResolveUserId();
        var contentItemManager = contentItemManagerFactory.Create(userId);

        await contentItemManager.TryCreateDraft(command.ContentItemId, languageName, cancellationToken);
        await contentItemManager.TryUpdateDraft(command.ContentItemId, languageName, new ContentItemData(fieldData), cancellationToken);

        if (isPublished)
        {
            await contentItemManager.TryPublish(command.ContentItemId, languageName, cancellationToken);
        }

        return RelayCommandResult.Ok($"Re-optimized asset '{command.FieldName}' on content item {command.ContentItemId}.");
    }
}