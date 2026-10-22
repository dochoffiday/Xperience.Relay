using System.Text.Json;
using CMS.ContentEngine;
using CMS.Websites;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

public class QueryWebPageItemsCommandHandler(
    IContentQueryExecutor executor,
    IWebPageUrlRetriever webPageUrlRetriever,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<QueryWebPageItemsCommand>
{
    private const string RelativeUrlKey = "WebPageRelativeUrl";
    private const string AbsoluteUrlKey = "WebPageAbsoluteUrl";
    private const string StatusKey = "WebPageStatus";
    private const string IsPublishedKey = "WebPageIsPublished";

    // Not UrlPathColumns(): it adds its columns without deduping against the caller's own Columns.
    private static readonly string[] UrlColumns =
    [
        nameof(IWebPageContentQueryDataContainer.WebPageUrlPath),
        nameof(IWebPageContentQueryDataContainer.WebPageItemTreePath),
        nameof(IWebPageContentQueryDataContainer.WebPageItemWebsiteChannelID),
    ];

    private static readonly string[] StatusColumns =
    [
        nameof(IWebPageContentQueryDataContainer.ContentItemCommonDataVersionStatus),
    ];

    private readonly RelayKenticoOptions _options = options.Value;

    private readonly record struct UrlData(string? UrlPath, string? TreePath, int ChannelId);

    private sealed record PageRow(Dictionary<string, JsonElement> Values, VersionStatus Status, UrlData Url);

    public async Task<RelayCommandResult> HandleAsync(QueryWebPageItemsCommand command, CancellationToken cancellationToken = default)
    {
        var websiteChannelName = command.WebsiteChannelName ?? _options.DefaultWebsiteChannelName;

        if (string.IsNullOrWhiteSpace(websiteChannelName))
        {
            return RelayCommandResult.Fail("WebsiteChannelName is required. Set it on the command or configure RelayKenticoOptions.DefaultWebsiteChannelName.");
        }

        if (command.ContentTypeNames is not { Count: > 0 })
        {
            return RelayCommandResult.Fail("ContentTypeNames must list at least one content type.");
        }

        if (command.Columns is not { Count: > 0 })
        {
            return RelayCommandResult.Fail("Columns must list at least one column.");
        }

        var problem = ContentQueryChecker.Check(command, webPages: true, command.IncludeUrls);

        if (problem is not null)
        {
            return RelayCommandResult.Fail(problem);
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var columns = Combine(
            command.Columns,
            command.IncludeUrls ? UrlColumns : [],
            command.IncludeStatus ? StatusColumns : []);

        var pages = (await executor.GetWebPageResult(
            BuildQuery(command, websiteChannelName, languageName, columns),
            container => new PageRow(
                QueryItemsHelpers.ExtractRow(command.Columns, container.TryGetValue<object>),
                command.IncludeStatus ? container.ContentItemCommonDataVersionStatus : default,
                command.IncludeUrls ? ReadUrlData(container) : default),
            new ContentQueryExecutionOptions { ForPreview = !command.PublishedOnly, IncludeSecuredItems = true },
            cancellationToken)).ToList();

        var items = new List<Dictionary<string, JsonElement>>();

        foreach (var page in pages)
        {
            if (command.IncludeStatus)
            {
                page.Values[StatusKey] = JsonSerializer.SerializeToElement(page.Status.ToString());

                // A Draft is always a change on top of a published version (a page that was never published is an
                // InitialDraft), so the latest version's status is all it takes to know if a page is published.
                page.Values[IsPublishedKey] = JsonSerializer.SerializeToElement(page.Status is VersionStatus.Published or VersionStatus.Draft);
            }

            if (command.IncludeUrls)
            {
                var retrieved = await webPageUrlRetriever.Retrieve(
                    page.Url.UrlPath ?? string.Empty,
                    page.Url.TreePath ?? string.Empty,
                    page.Url.ChannelId,
                    languageName,
                    cancellationToken);

                page.Values[RelativeUrlKey] = JsonSerializer.SerializeToElement(retrieved.RelativePath);
                page.Values[AbsoluteUrlKey] = JsonSerializer.SerializeToElement(retrieved.AbsoluteUrl);
            }

            items.Add(page.Values);
        }

        return RelayCommandResult.Ok(data: new QueryItemsResult { Items = items });
    }

    private static ContentItemQueryBuilder BuildQuery(
        QueryWebPageItemsCommand command,
        string websiteChannelName,
        string languageName,
        string[] columns)
    {
        var builder = new ContentItemQueryBuilder();

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

        return builder;
    }

    private static UrlData ReadUrlData(IWebPageContentQueryDataContainer container) => new(
        container.WebPageUrlPath,
        container.WebPageItemTreePath,
        container.WebPageItemWebsiteChannelID);

    private static string[] Combine(params IEnumerable<string>[] columnSets) =>
        columnSets.SelectMany(set => set).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
}
