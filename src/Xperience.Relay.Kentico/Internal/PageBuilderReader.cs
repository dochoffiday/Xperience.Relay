using System.Text.Json;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Websites.Internal;
using Xperience.Relay.Contracts;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>The Page Builder JSON of a page in one language, as Kentico stores it (text).</summary>
public sealed record StoredPageBuilder(string? Widgets, string? Template)
{
    /// <summary>The same data for a result: null when the page has none, and a part that isn't valid JSON counts as none.</summary>
    public PageBuilderData? ToData()
    {
        var widgets = Parse(Widgets);
        var template = Parse(Template);

        return widgets is null && template is null ? null : new PageBuilderData { Widgets = widgets, Template = template };
    }

    private static JsonElement? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Null ? null : document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>
/// Reads the Page Builder data of a page: its widgets and page template, which Kentico keeps as JSON text in the columns
/// of the page's latest version.
/// </summary>
public sealed class PageBuilderReader(
    IInfoProvider<WebPageItemInfo> pageProvider,
    IInfoProvider<ContentLanguageInfo> languageProvider,
    IInfoProvider<ContentItemCommonDataInfo> commonDataProvider)
{
    private const string WidgetsColumn = "ContentItemCommonDataVisualBuilderWidgets";
    private const string TemplateColumn = "ContentItemCommonDataVisualBuilderTemplateConfiguration";

    /// <summary>Returns null when the page, or its variant in the language, isn't there.</summary>
    public StoredPageBuilder? Read(int webPageId, string languageName)
    {
        var page = pageProvider.Get()
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), webPageId)
            .FirstOrDefault();

        var language = languageProvider.Get()
            .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), languageName)
            .FirstOrDefault();

        if (page is null || language is null)
        {
            return null;
        }

        var latest = commonDataProvider.Get()
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID), page.WebPageItemContentItemID)
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentLanguageID), language.ContentLanguageID)
            .WhereTrue(nameof(ContentItemCommonDataInfo.ContentItemCommonDataIsLatest))
            .FirstOrDefault();

        return latest is null
            ? null
            : new StoredPageBuilder(Text(latest.GetValue(WidgetsColumn)), Text(latest.GetValue(TemplateColumn)));
    }

    private static string? Text(object? value) =>
        value is null or DBNull || string.IsNullOrWhiteSpace(value.ToString()) ? null : value.ToString();
}
