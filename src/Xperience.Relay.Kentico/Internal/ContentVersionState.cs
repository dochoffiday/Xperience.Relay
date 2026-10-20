using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// Where a content item (or page) stands in one language: the status of its latest version, and whether any of its
/// versions is published (a published page with a newer draft has both).
/// </summary>
public readonly record struct ContentVersionState(VersionStatus Latest, bool HasPublishedVersion);

public sealed class ContentVersionReader(
    IInfoProvider<ContentItemInfo> contentItemProvider,
    IInfoProvider<ContentLanguageInfo> languageProvider,
    IInfoProvider<ContentItemCommonDataInfo> commonDataProvider)
{
    /// <summary>
    /// Returns why <paramref name="contentItemId"/> can't be used with the reusable content item verbs, or null if it can.
    /// Kentico's managers throw for a page's content item, so say so up front rather than as an exception.
    /// </summary>
    public string? CheckReusable(int contentItemId, string pageVerb)
    {
        var contentItem = contentItemProvider.Get()
            .WhereEquals(nameof(ContentItemInfo.ContentItemID), contentItemId)
            .FirstOrDefault();

        if (contentItem is null)
        {
            return $"Content item {contentItemId} was not found.";
        }

        return contentItem.ContentItemIsReusable
            ? null
            : $"Content item {contentItemId} is not a reusable content item. Use {pageVerb} for web pages.";
    }

    /// <summary>Returns null when the item has no version in that language, or the language doesn't exist.</summary>
    public ContentVersionState? Read(int contentItemId, string languageName)
    {
        var language = languageProvider.Get()
            .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), languageName)
            .FirstOrDefault();

        if (language is null)
        {
            return null;
        }

        var versions = commonDataProvider.Get()
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID), contentItemId)
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentLanguageID), language.ContentLanguageID)
            .Columns(
                nameof(ContentItemCommonDataInfo.ContentItemCommonDataIsLatest),
                nameof(ContentItemCommonDataInfo.ContentItemCommonDataVersionStatus))
            .ToList();

        var latest = versions.FirstOrDefault(v => v.ContentItemCommonDataIsLatest);

        return latest is null
            ? null
            : new ContentVersionState(
                latest.ContentItemCommonDataVersionStatus,
                versions.Any(v => v.ContentItemCommonDataVersionStatus == VersionStatus.Published));
    }
}
