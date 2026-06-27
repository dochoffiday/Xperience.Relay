using CMS.ContentEngine;
using CMS.DataEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Fetches a reusable content item's system fields (via <see cref="GetContentInfoCommandHandler"/>)
/// plus its content-type-specific field data, using <see cref="ClassStructureInfo"/> to discover the
/// content type's custom field names.
/// </summary>
public class GetContentCommandHandler : IRelayCommandHandler<GetContentCommand>
{
    private static readonly string[] SystemColumnPrefixes = { "ContentItem" };

    private readonly GetContentInfoCommandHandler _contentInfoHandler;
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly RelayKenticoOptions _options;

    public GetContentCommandHandler(
        GetContentInfoCommandHandler contentInfoHandler,
        IContentQueryExecutor contentQueryExecutor,
        IOptions<RelayKenticoOptions> options)
    {
        _contentInfoHandler = contentInfoHandler;
        _contentQueryExecutor = contentQueryExecutor;
        _options = options.Value;
    }

    public async Task<RelayCommandResult> HandleAsync(GetContentCommand command, CancellationToken cancellationToken = default)
    {
        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var info = await _contentInfoHandler.FetchContentInfoAsync(command.ContentItemId, cancellationToken);
        if (info is null)
        {
            return RelayCommandResult.Fail($"Content item {command.ContentItemId} was not found.");
        }

        var fieldColumnNames = ClassStructureInfo.GetClassInfo(info.ContentType).ColumnNames
            .Where(name => !SystemColumnPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var builder = new ContentItemQueryBuilder()
            .ForContentType(info.ContentType, _ => { })
            .InLanguage(languageName)
            .Parameters(p => p.Where(w => w.WhereEquals("ContentItemID", command.ContentItemId)));

        var rows = await _contentQueryExecutor.GetResult(
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

        var data = new ContentData
        {
            ContentItemId = info.ContentItemId,
            ContentItemGuid = info.ContentItemGuid,
            Name = info.Name,
            ContentType = info.ContentType,
            LanguageName = languageName,
            WorkspaceName = info.WorkspaceName,
            ContentFolderPath = info.ContentFolderPath,
            Fields = fieldData,
        };

        return RelayCommandResult.Ok(data: data);
    }
}
