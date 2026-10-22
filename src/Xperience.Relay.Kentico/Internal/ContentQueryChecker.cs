using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Websites.Internal;
using Xperience.Relay.Contracts.Commands;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// Checks what <c>query-web-page-items</c> and <c>query-reusable-items</c> are asked to select and to filter on, before
/// any query is built: that the content types exist, that every column is a plain name, and that the columns exist on
/// every content type, so a mistake is answered in words instead of with an error from the database.
/// See <see cref="QueryInputChecker"/>.
/// </summary>
internal static class ContentQueryChecker
{
    /// <param name="command">The query command.</param>
    /// <param name="webPages">True for web page content types, whose queries also have the columns of a web page.</param>
    /// <param name="includeUrls">True when the query asks for the URL columns of web pages.</param>
    /// <returns>Null when the query is fine, otherwise every problem found.</returns>
    public static string? Check(QueryItemsCommandBase command, bool webPages, bool includeUrls)
    {
        var shape = QueryInputChecker.CheckQuery(command.ContentTypeNames, command.Columns, command.WhereEquals);

        if (shape is not null)
        {
            return shape;
        }

        var filterColumns = command.WhereEquals?.Keys.ToList() ?? [];
        var problems = new List<string>();

        foreach (var contentTypeName in command.ContentTypeNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var known = KnownColumns(contentTypeName, webPages, includeUrls);

            if (known is null)
            {
                problems.Add($"Content type '{contentTypeName}' was not found.");
                continue;
            }

            var unknown = ObjectColumnChecker.Check(contentTypeName, known, command.Columns.Concat(filterColumns), "content type");

            if (unknown is not null)
            {
                problems.Add(unknown);
            }
        }

        // The query filters on the columns it selects, and Kentico fails on one that isn't among them.
        var unlisted = filterColumns
            .Where(column => !command.Columns.Contains(column, StringComparer.OrdinalIgnoreCase))
            .Select(column => $"'{column}'")
            .ToList();

        if (unlisted.Count > 0)
        {
            problems.Add(unlisted.Count == 1
                ? $"The WhereEquals column {unlisted[0]} must also be listed in Columns."
                : $"The WhereEquals columns {string.Join(", ", unlisted)} must also be listed in Columns.");
        }

        return problems.Count == 0 ? null : string.Join(" ", problems);
    }

    /// <summary>
    /// The columns a query of the content type can select: its own fields (including those of its reusable field schemas),
    /// the columns every content item has, and for web pages the columns of a page and of its URL.
    /// </summary>
    /// <returns>Null when there is no such content type.</returns>
    private static HashSet<string>? KnownColumns(string contentTypeName, bool webPages, bool includeUrls)
    {
        // The merged form of the type and its reusable field schemas, as the checks on content writes use.
        var form = FormHelper.GetFormInfo(ReusableFieldSchemaUtils.GetPrefixedContentTypeName(contentTypeName), clone: false);

        if (form is null)
        {
            return null;
        }

        var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        known.UnionWith(form.GetFields(visible: true, invisible: true, includeSystem: true).Select(field => field.Name));
        known.UnionWith(ClassStructureInfo.GetClassInfo(contentTypeName)?.ColumnNames ?? []);
        known.UnionWith(ContentItemInfo.TYPEINFO.ClassStructureInfo.ColumnNames);
        known.UnionWith(ContentItemCommonDataInfo.TYPEINFO.ClassStructureInfo.ColumnNames);

        if (webPages)
        {
            known.UnionWith(WebPageItemInfo.TYPEINFO.ClassStructureInfo.ColumnNames);

            if (includeUrls)
            {
                known.Add("WebPageUrlPath");
                known.Add("WebPageUrlPathContentLanguageID");
            }
        }

        return known;
    }
}
