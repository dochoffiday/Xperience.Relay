using CMS.Helpers;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Microsoft.Extensions.DependencyInjection;
using Xperience.Relay.Contracts;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// The page templates of the site, as Kentico's own administration lists them for a page. They are registered by the
/// site's code, so a site that registers none, such as one that doesn't use Page Builder, simply has none. The provider
/// that knows them is looked up when it is needed.
/// </summary>
public sealed class PageTemplateCatalog(IServiceProvider services)
{
    private IPageTemplateDefinitionProvider? Provider => services.GetService<IPageTemplateDefinitionProvider>();

    /// <summary>The templates a page can use, by name. Empty when the site registers none.</summary>
    /// <param name="parentWebPageItemId">The page's parent, or 0 for the root of the channel.</param>
    /// <param name="contentTypeName">Code name of the page's content type.</param>
    /// <param name="languageName">The language of the page.</param>
    public List<PageTemplateResult> For(int parentWebPageItemId, string contentTypeName, string languageName)
    {
        if (Provider is not { } provider)
        {
            return [];
        }

        var context = new PageTemplateFilterContext(parentWebPageItemId, contentTypeName, languageName);

        return provider.GetFilteredTemplates(context)
            .Select(definition => new PageTemplateResult
            {
                Identifier = definition.Identifier,
                Name = Localize(definition.Name) ?? definition.Identifier,
                Description = Localize(definition.Description),
                IconClass = string.IsNullOrWhiteSpace(definition.IconClass) ? null : definition.IconClass,
                ContentTypeNames = definition.ContentTypeNames?.ToList() ?? [],
            })
            .OrderBy(template => template.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(template => template.Identifier, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Names and descriptions can be localization keys, such as <c>{$dancinggoat.articletemplate.title$}</c>.</summary>
    private static string? Localize(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : ResHelper.LocalizeString(text);
}
