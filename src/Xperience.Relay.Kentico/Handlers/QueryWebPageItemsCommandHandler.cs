using CMS.ContentEngine;
using CMS.Websites;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class QueryWebPageItemsCommandHandler(
    IContentQueryExecutor executor,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<QueryWebPageItemsCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(QueryWebPageItemsCommand command, CancellationToken cancellationToken = default)
    {
        var websiteChannelName = command.WebsiteChannelName ?? _options.DefaultWebsiteChannelName;

        if (string.IsNullOrWhiteSpace(websiteChannelName))
        {
            return RelayCommandResult.Fail("WebsiteChannelName is required. Set it on the command or configure RelayKenticoOptions.DefaultWebsiteChannelName.");
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var builder = new ContentItemQueryBuilder();

        foreach (var contentTypeName in command.ContentTypeNames)
        {
            builder.ForContentType(contentTypeName, q =>
            {
                q.Columns(command.Columns.ToArray());
                q.ForWebsite(websiteChannelName, PathMatch.Section("/"), false);
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

        var rows = await executor.GetWebPageResult(
            builder,
            container => QueryItemsHelpers.ExtractRow(command.Columns, container.TryGetValue<object>),
            queryOptions,
            cancellationToken);

        return RelayCommandResult.Ok(data: new QueryItemsResult { Items = rows.ToList() });
    }
}
