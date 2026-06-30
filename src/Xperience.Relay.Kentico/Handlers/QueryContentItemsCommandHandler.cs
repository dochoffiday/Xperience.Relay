using System.Text.Json;
using CMS.ContentEngine;
using CMS.Websites;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class QueryContentItemsCommandHandler(
    IContentQueryExecutor executor,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<QueryContentItemsCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(QueryContentItemsCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ContentTypeName))
        {
            return RelayCommandResult.Fail("ContentTypeName must not be empty.");
        }

        if (command.Columns.Count == 0)
        {
            return RelayCommandResult.Fail("At least one column must be specified.");
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var builder = new ContentItemQueryBuilder()
            .ForContentType(command.ContentTypeName, q =>
            {
                q.Columns(command.Columns.ToArray());
            })
            .InLanguage(languageName);

        if (command.WhereEquals?.Count > 0)
        {
            builder.Parameters(p =>
            {
                foreach (var (col, val) in command.WhereEquals)
                {
                    p.Where(w => w.WhereEquals(col, GetScalarValue(val)));
                }
            });
        }

        var queryOptions = new ContentQueryExecutionOptions { ForPreview = true, IncludeSecuredItems = true };
        var columns = command.Columns;

        IEnumerable<Dictionary<string, JsonElement>> rows;

        if (command.IsWebPage)
        {
            // IWebPageContentQueryDataContainer and IContentQueryDataContainer both have TryGetValue
            // but are distinct types, so the extraction lambda must be typed separately for each path.
            rows = await executor.GetWebPageResult(
                builder,
                container =>
                {
                    var dict = new Dictionary<string, JsonElement>(columns.Count);
                    foreach (var col in columns)
                    {
                        if (container.TryGetValue<object>(col, out var val))
                        {
                            dict[col] = JsonDocument.Parse(JsonSerializer.Serialize(val)).RootElement.Clone();
                        }
                    }
                    return dict;
                },
                queryOptions,
                cancellationToken);
        }
        else
        {
            rows = await executor.GetResult(
                builder,
                container =>
                {
                    var dict = new Dictionary<string, JsonElement>(columns.Count);
                    foreach (var col in columns)
                    {
                        if (container.TryGetValue<object>(col, out var val))
                        {
                            dict[col] = JsonDocument.Parse(JsonSerializer.Serialize(val)).RootElement.Clone();
                        }
                    }
                    return dict;
                },
                queryOptions,
                cancellationToken);
        }

        return RelayCommandResult.Ok(data: new QueryContentItemsResult { Items = rows.ToList() });
    }

    private static object? GetScalarValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number when element.TryGetInt32(out var i) => i,
        JsonValueKind.Number when element.TryGetInt64(out var l) => l,
        JsonValueKind.Number => element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };
}
