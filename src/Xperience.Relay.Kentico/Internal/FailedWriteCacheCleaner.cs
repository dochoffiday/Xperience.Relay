using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// Kentico changes its cached copy of an item before it saves it, and only drops that copy after a save that worked. A
/// save that fails (the database refusing a value, say) therefore leaves the refused value in the cache, and the next
/// edit of the item writes it again. This drops the cached copies of an item's latest version after such a failure.
/// </summary>
public sealed class FailedWriteCacheCleaner(
    IInfoProvider<ContentItemInfo> contentItemProvider,
    IInfoProvider<ContentLanguageInfo> languageProvider,
    IInfoProvider<ContentItemCommonDataInfo> commonDataProvider,
    IContentItemDataInfoProviderAccessor dataProviderAccessor)
{
    public void Forget(int contentItemId, string languageName)
    {
        var contentItem = contentItemProvider.Get()
            .WhereEquals(nameof(ContentItemInfo.ContentItemID), contentItemId)
            .FirstOrDefault();

        var language = languageProvider.Get()
            .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), languageName)
            .FirstOrDefault();

        if (contentItem is null || language is null)
        {
            return;
        }

        var commonData = commonDataProvider.Get()
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID), contentItemId)
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentLanguageID), language.ContentLanguageID)
            .WhereTrue(nameof(ContentItemCommonDataInfo.ContentItemCommonDataIsLatest))
            .FirstOrDefault();

        if (commonData is null)
        {
            return;
        }

        commonData.TouchKeys();

        var className = DataClassInfoProvider.GetClassName(contentItem.ContentItemContentTypeID);

        if (string.IsNullOrEmpty(className))
        {
            return;
        }

        dataProviderAccessor.Get(className).Get()
            .WhereEquals(nameof(ContentItemDataInfo.ContentItemDataCommonDataID), commonData.ContentItemCommonDataID)
            .FirstOrDefault()
            ?.TouchKeys();
    }
}
