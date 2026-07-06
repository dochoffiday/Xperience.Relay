using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Fetches a web page's system fields (via <see cref="GetPageInfoCommandHandler"/>) plus its
/// content-type-specific field data. Field names come from <see cref="ClassStructureInfo"/> for the
/// page's content type, since custom fields vary per type and aren't otherwise enumerable from a
/// content item query result.
/// </summary>
public class GetPageCommandHandler : IRelayCommandHandler<GetPageCommand>
{
    private static readonly string[] SystemColumnPrefixes = { "WebPageItem", "WebPageUrlPath", "ContentItem" };

    private readonly GetPageInfoCommandHandler _pageInfoHandler;
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly RelayKenticoOptions _optionsValue;

    public GetPageCommandHandler(
        GetPageInfoCommandHandler pageInfoHandler,
        IContentQueryExecutor contentQueryExecutor,
        Microsoft.Extensions.Options.IOptions<RelayKenticoOptions> options)
    {
        _pageInfoHandler = pageInfoHandler;
        _contentQueryExecutor = contentQueryExecutor;
        _optionsValue = options.Value;
    }

    public async Task<RelayCommandResult> HandleAsync(GetPageCommand command, CancellationToken cancellationToken = default)
    {
        var languageName = command.LanguageName ?? _optionsValue.DefaultLanguageName;

        var info = _pageInfoHandler.FetchWebPageInfo(command.WebPageId, languageName);
        if (info is null)
        {
            return RelayCommandResult.Fail($"Web page {command.WebPageId} was not found.");
        }

        var fieldColumnNames = ClassStructureInfo.GetClassInfo(info.ContentType).ColumnNames
            .Where(name => !SystemColumnPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var builder = new ContentItemQueryBuilder()
            .ForContentType(info.ContentType, _ => { })
            .InLanguage(languageName)
            .Parameters(p => p.Where(w => w.WhereEquals("WebPageItemID", command.WebPageId)));

        var rows = await _contentQueryExecutor.GetWebPageResult(
            builder,
            container =>
            {
                var fields = new Dictionary<string, object?>();
                foreach (var columnName in fieldColumnNames)
                {
                    fields[columnName] = container.TryGetValue<object?>(columnName, out var value) ? value : null;
                }
                return fields;
            },
            cancellationToken: cancellationToken);

        var fieldData = rows.FirstOrDefault() ?? new Dictionary<string, object?>();

        var data = new WebPageData
        {
            WebPageId = info.WebPageId,
            WebPageGuid = info.WebPageGuid,
            Name = info.Name,
            ContentType = info.ContentType,
            TreePath = info.TreePath,
            ParentWebPageId = info.ParentWebPageId,
            LanguageName = info.LanguageName,
            ChannelName = info.ChannelName,
            Fields = fieldData,
        };

        return RelayCommandResult.Ok(data: data);
    }
}
