using CMS.ContentEngine;
using CMS.ContentEngine.Internal;

namespace Xperience.Relay.Kentico.Extensions;

public static class ContentItemCommonDataInfoExtensions
{
    public static bool IsPublished(this ContentItemCommonDataInfo? contentItem)
    {
        return contentItem != null && contentItem.ContentItemCommonDataVersionStatus == VersionStatus.Published;
    }
}
