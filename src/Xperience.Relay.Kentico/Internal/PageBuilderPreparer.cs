using Xperience.Relay.Contracts;
using Xperience.Relay.Kentico.Handlers;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>What to hand to Kentico for a page's Page Builder data, or why it can't be done.</summary>
/// <param name="Widgets">The widgets JSON, or null when there is none.</param>
/// <param name="Template">The page template JSON, or null when there is none.</param>
/// <param name="Error">Why the data can't be used; null when it can.</param>
public sealed record PageBuilderPlan(string? Widgets, string? Template, string? Error)
{
    public static PageBuilderPlan Fail(string error) => new(null, null, error);
}

/// <summary>
/// Turns the Page Builder data of a create or update command into what Kentico is given, after checking it: the structure
/// of the widgets, and that the template is one the page can use. An update that sets only one part keeps the other as
/// the page has it.
/// </summary>
public sealed class PageBuilderPreparer(
    PageTemplateCatalog templates,
    PageBuilderReader reader,
    GetPageInfoCommandHandler pageInfoHandler)
{
    /// <summary>For a page that exists, in the language of the edit.</summary>
    public PageBuilderPlan ForExistingPage(PageBuilderData data, int webPageId, string languageName)
    {
        var page = pageInfoHandler.FetchWebPageInfo(webPageId, languageName);

        return page is null
            ? PageBuilderPlan.Fail($"Web page {webPageId} was not found.")
            : Prepare(data, page.ContentType, page.ParentWebPageId ?? 0, languageName, reader.Read(webPageId, languageName));
    }

    /// <summary>For a page that is about to be created.</summary>
    public PageBuilderPlan ForNewPage(PageBuilderData data, string contentTypeName, int parentWebPageItemId, string languageName) =>
        Prepare(data, contentTypeName, parentWebPageItemId, languageName, current: null);

    private PageBuilderPlan Prepare(
        PageBuilderData data,
        string contentTypeName,
        int parentWebPageItemId,
        string languageName,
        StoredPageBuilder? current)
    {
        if (PageBuilderChecker.Check(data) is { } problem)
        {
            return PageBuilderPlan.Fail(problem);
        }

        if (PageBuilderChecker.TemplateIdentifier(data.Template) is { } identifier)
        {
            var currentIdentifier = PageBuilderChecker.TemplateIdentifier(current?.ToData()?.Template);
            var available = templates.For(parentWebPageItemId, contentTypeName, languageName);

            // The template the page has stays allowed, even when the site has stopped offering it to others.
            if (identifier != currentIdentifier && available.All(template => template.Identifier != identifier))
            {
                return PageBuilderPlan.Fail(
                    $"Page template '{identifier}' isn't available for content type '{contentTypeName}'. \"get-page-templates\" lists the ones that are.");
            }
        }

        return new PageBuilderPlan(
            data.Widgets?.GetRawText() ?? current?.Widgets,
            data.Template?.GetRawText() ?? current?.Template,
            Error: null);
    }
}
