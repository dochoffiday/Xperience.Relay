using System.Text.Json;
using CMS.ContentEngine;
using CMS.Websites;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class QueryWebPageItemsCommandHandler(
    IContentQueryExecutor executor,
    IWebPageUrlRetriever webPageUrlRetriever,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<QueryWebPageItemsCommand>
{
    private const string RelativeUrlKey = "WebPageRelativeUrl";
    private const string AbsoluteUrlKey = "WebPageAbsoluteUrl";

    // Not UrlPathColumns(): it adds its columns without deduping against the caller's own Columns.
    private static readonly string[] UrlColumns =
    [
        nameof(IWebPageContentQueryDataContainer.WebPageUrlPath),
        nameof(IWebPageContentQueryDataContainer.WebPageItemTreePath),
        nameof(IWebPageContentQueryDataContainer.WebPageItemWebsiteChannelID),
    ];

    private readonly RelayKenticoOptions _options = options.Value;

    private sealed record PageRow(Dictionary<string, JsonElement> Values, string? UrlPath, string? TreePath, int ChannelId);

    public async Task<RelayCommandResult> HandleAsync(QueryWebPageItemsCommand command, CancellationToken cancellationToken = default)
    {
        var websiteChannelName = command.WebsiteChannelName ?? _options.DefaultWebsiteChannelName;

        if (string.IsNullOrWhiteSpace(websiteChannelName))
        {
            return RelayCommandResult.Fail("WebsiteChannelName is required. Set it on the command or configure RelayKenticoOptions.DefaultWebsiteChannelName.");
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var builder = new ContentItemQueryBuilder();

        var columns = command.IncludeUrls
            ? command.Columns.Concat(UrlColumns).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            : command.Columns.ToArray();

        foreach (var contentTypeName in command.ContentTypeNames)
        {
            builder.ForContentType(contentTypeName, q =>
            {
                q.Columns(columns);
                q.ForWebsite(websiteChannelName, PathMatch.Section("/"), command.IncludeUrls);
            });
        }

        builder.InLanguage(languageName);

        if (command.WhereEquals?.Count > 0)
        {
            builder.Parameters(p =>
            {
                foreach (var (col, val) in command.WhereEquals)
                {
                    p.Where(w => w.WhereEquals(col, QueryItemsHelpers.GetScalarValue(val)));
                }
            });
        }

        var queryOptions = new ContentQueryExecutionOptions { ForPreview = true, IncludeSecuredItems = true };

        var pages = await executor.GetWebPageResult(
            builder,
            container => new PageRow(
                QueryItemsHelpers.ExtractRow(command.Columns, container.TryGetValue<object>),
                command.IncludeUrls ? container.WebPageUrlPath : null,
                command.IncludeUrls ? container.WebPageItemTreePath : null,
                command.IncludeUrls ? container.WebPageItemWebsiteChannelID : 0),
            queryOptions,
            cancellationToken);

        var items = new List<Dictionary<string, JsonElement>>();

        foreach (var page in pages)
        {
            if (command.IncludeUrls)
            {
                var url = await webPageUrlRetriever.Retrieve(
                    page.UrlPath ?? string.Empty,
                    page.TreePath ?? string.Empty,
                    page.ChannelId,
                    languageName,
                    cancellationToken);

                page.Values[RelativeUrlKey] = JsonSerializer.SerializeToElement(url.RelativePath);
                page.Values[AbsoluteUrlKey] = JsonSerializer.SerializeToElement(url.AbsoluteUrl);
            }

            items.Add(page.Values);
        }

        return RelayCommandResult.Ok(data: new QueryItemsResult { Items = items });
    }
}
