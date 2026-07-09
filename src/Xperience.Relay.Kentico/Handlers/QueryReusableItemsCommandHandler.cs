using CMS.ContentEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class QueryReusableItemsCommandHandler(
    IContentQueryExecutor executor,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<QueryReusableItemsCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(QueryReusableItemsCommand command, CancellationToken cancellationToken = default)
    {
        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var builder = new ContentItemQueryBuilder();

        foreach (var contentTypeName in command.ContentTypeNames)
        {
            builder.ForContentType(contentTypeName, q =>
            {
                q.Columns(command.Columns.ToArray());
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

        var rows = await executor.GetResult(
            builder,
            container => QueryItemsHelpers.ExtractRow(command.Columns, container.TryGetValue<object>),
            queryOptions,
            cancellationToken);

        return RelayCommandResult.Ok(data: new QueryItemsResult { Items = rows.ToList() });
    }
}
