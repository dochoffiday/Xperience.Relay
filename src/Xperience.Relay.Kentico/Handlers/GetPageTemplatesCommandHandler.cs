using CMS.DataEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Lists the page templates a page can use, the way Kentico's administration offers them: the templates registered by the
/// site's code, narrowed by their content types and by any filter the site has added, for the page's content type, parent
/// and language.
/// </summary>
public class GetPageTemplatesCommandHandler(
    PageTemplateCatalog catalog,
    PageBuilderReader reader,
    GetPageInfoCommandHandler pageInfoHandler,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<GetPageTemplatesCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public Task<RelayCommandResult> HandleAsync(GetPageTemplatesCommand command, CancellationToken cancellationToken = default)
    {
        var hasPage = command.WebPageId.HasValue;
        var hasContentType = !string.IsNullOrWhiteSpace(command.ContentTypeName);

        if (hasPage == hasContentType)
        {
            return Task.FromResult(RelayCommandResult.Fail("Give either WebPageId or ContentTypeName."));
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;
        string contentTypeName;
        int parentWebPageItemId;
        string? currentTemplate = null;

        if (command.WebPageId is { } webPageId)
        {
            var page = pageInfoHandler.FetchWebPageInfo(webPageId, languageName);

            if (page is null)
            {
                return Task.FromResult(RelayCommandResult.Fail($"Web page {webPageId} was not found."));
            }

            contentTypeName = page.ContentType;
            parentWebPageItemId = page.ParentWebPageId ?? 0;
            currentTemplate = PageBuilderChecker.TemplateIdentifier(reader.Read(webPageId, languageName)?.ToData()?.Template);
        }
        else
        {
            contentTypeName = command.ContentTypeName!;
            var contentType = QueryInputChecker.IsCodeName(contentTypeName) ? DataClassInfoProvider.GetDataClassInfo(contentTypeName) : null;

            if (contentType is null || contentType.ClassContentTypeType != ClassContentTypeType.WEBSITE)
            {
                return Task.FromResult(RelayCommandResult.Fail(
                    $"Content type '{QueryInputChecker.Printable(contentTypeName)}' was not found, or isn't a web page content type."));
            }

            parentWebPageItemId = command.ParentWebPageItemId;

            if (parentWebPageItemId != 0 && pageInfoHandler.FetchWebPageInfo(parentWebPageItemId, languageName) is null)
            {
                return Task.FromResult(RelayCommandResult.Fail($"Web page {parentWebPageItemId} (the parent) was not found."));
            }
        }

        var templates = catalog.For(parentWebPageItemId, contentTypeName, languageName);

        return Task.FromResult(RelayCommandResult.Ok(
            message: $"Found {templates.Count} page template(s) for content type '{contentTypeName}'.",
            data: new GetPageTemplatesResult { Templates = templates, CurrentTemplate = currentTemplate }));
    }
}
