using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Websites;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class SearchContentCommandHandler(
    IContentQueryExecutor executor,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<SearchContentCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    private static readonly HashSet<string> TextDataTypes =
    [
        FieldDataType.Text,
        FieldDataType.LongText,
        FieldDataType.RichTextHTML
    ];

    public async Task<RelayCommandResult> HandleAsync(SearchContentCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ContentTypeName))
        {
            return RelayCommandResult.Fail("ContentTypeName is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Filter))
        {
            return RelayCommandResult.Fail("Filter is required.");
        }

        var dataClass = DataClassInfoProvider
            .GetClasses()
            .WhereEquals(nameof(DataClassInfo.ClassName), command.ContentTypeName)
            .FirstOrDefault();

        if (dataClass is null)
        {
            return RelayCommandResult.Fail($"Content type '{command.ContentTypeName}' was not found.");
        }

        var formInfo = new FormInfo(dataClass.ClassFormDefinition);
        var textColumns = formInfo
            .GetFields(visible: true, invisible: true, includeSystem: false, onlyPrimaryKeys: false, includeDummyFields: false)
            .Where(f => TextDataTypes.Contains(f.DataType))
            .Select(f => f.Name)
            .ToList();

        if (textColumns.Count == 0)
        {
            return RelayCommandResult.Ok(
                message: $"Content type '{command.ContentTypeName}' has no searchable text fields.",
                data: new SearchContentResult());
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;
        var isWebPage = dataClass.ClassContentTypeType == ClassContentTypeType.WEBSITE;
        var queryOptions = new ContentQueryExecutionOptions { ForPreview = false, IncludeSecuredItems = true };

        if (isWebPage)
        {
            var channelName = command.WebsiteChannelName ?? _options.DefaultWebsiteChannelName;

            if (string.IsNullOrWhiteSpace(channelName))
            {
                return RelayCommandResult.Fail("WebsiteChannelName is required for web page content types. Set it on the command or configure RelayKenticoOptions.DefaultWebsiteChannelName.");
            }

            var allColumns = textColumns
                .Concat([nameof(IWebPageContentQueryDataContainer.WebPageItemContentItemID), nameof(IWebPageContentQueryDataContainer.WebPageItemName), nameof(IWebPageContentQueryDataContainer.WebPageItemTreePath)])
                .Distinct()
                .ToArray();

            var builder = new ContentItemQueryBuilder()
                .ForContentType(command.ContentTypeName, q =>
                {
                    q.Columns(allColumns);
                    q.ForWebsite(channelName, PathMatch.Section("/"), false);
                })
                .InLanguage(languageName);

            var filter = command.Filter;

            var rawResults = await executor.GetWebPageResult<SearchContentMatch?>(
                builder,
                container =>
                {
                    var matchedFields = ExtractMatchingFields(textColumns, container.TryGetValue<object>, filter);

                    if (matchedFields.Count == 0)
                    {
                        return null;
                    }

                    return new SearchContentMatch
                    {
                        ContentItemId = container.WebPageItemContentItemID,
                        ContentItemName = container.WebPageItemName,
                        ContentTypeName = command.ContentTypeName,
                        Location = container.WebPageItemTreePath,
                        MatchedFields = matchedFields
                    };
                },
                queryOptions,
                cancellationToken);

            var matches = rawResults.OfType<SearchContentMatch>().ToList();

            return RelayCommandResult.Ok(
                message: $"Found {matches.Count} matching item(s) in '{command.ContentTypeName}'.",
                data: new SearchContentResult { Matches = matches }
            );
        }
        else
        {
            var allColumns = textColumns
                .Concat([nameof(IContentQueryDataContainer.ContentItemID), nameof(IContentQueryDataContainer.ContentItemName)])
                .Distinct()
                .ToArray();

            var builder = new ContentItemQueryBuilder()
                .ForContentType(command.ContentTypeName, q => q.Columns(allColumns))
                .InLanguage(languageName);

            var filter = command.Filter;

            var rawResults = await executor.GetResult<SearchContentMatch?>(
                builder,
                container =>
                {
                    var matchedFields = ExtractMatchingFields(textColumns, container.TryGetValue<object>, filter);
                    
                    if (matchedFields.Count == 0)
                    {
                        return null;
                    }

                    return new SearchContentMatch
                    {
                        ContentItemId = container.ContentItemID,
                        ContentItemName = container.ContentItemName,
                        ContentTypeName = container.ContentTypeName,
                        Location = null,
                        MatchedFields = matchedFields
                    };
                },
                queryOptions,
                cancellationToken);

            var matches = rawResults.OfType<SearchContentMatch>().ToList();

            return RelayCommandResult.Ok(
                message: $"Found {matches.Count} matching item(s) in '{command.ContentTypeName}'.",
                data: new SearchContentResult { Matches = matches }
            );
        }
    }

    private static Dictionary<string, string> ExtractMatchingFields(
        List<string> columns,
        QueryItemsHelpers.TryGetValueDelegate tryGetValue,
        string filter)
    {
        var result = new Dictionary<string, string>();
        foreach (var col in columns)
        {
            if (tryGetValue(col, out var val) && val is string s &&
                s.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                result[col] = s;
            }
        }
        return result;
    }
}
