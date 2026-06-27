using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

public class GetContentInfoCommandHandler : IRelayCommandHandler<GetContentInfoCommand>
{
    private readonly IContentItemManagerFactory _contentItemManagerFactory;
    private readonly ServiceAccountResolver _serviceAccountResolver;

    public GetContentInfoCommandHandler(
        IContentItemManagerFactory contentItemManagerFactory,
        ServiceAccountResolver serviceAccountResolver)
    {
        _contentItemManagerFactory = contentItemManagerFactory;
        _serviceAccountResolver = serviceAccountResolver;
    }

    public async Task<RelayCommandResult> HandleAsync(GetContentInfoCommand command, CancellationToken cancellationToken = default)
    {
        var info = await FetchContentInfoAsync(command.ContentItemId, cancellationToken);
        return info is null
            ? RelayCommandResult.Fail($"Content item {command.ContentItemId} was not found.")
            : RelayCommandResult.Ok(data: info);
    }

    internal async Task<ContentInfo?> FetchContentInfoAsync(int contentItemId, CancellationToken cancellationToken)
    {
        var userId = _serviceAccountResolver.ResolveUserId();
        var manager = _contentItemManagerFactory.Create(userId);

        CMS.ContentEngine.ContentItemMetadata metadata;
        try
        {
            // GetContentItemMetadata throws rather than returning null for a missing item.
            metadata = await manager.GetContentItemMetadata(contentItemId, cancellationToken);
        }
        catch
        {
            return null;
        }

        return new ContentInfo
        {
            ContentItemId = metadata.ContentItemID,
            ContentItemGuid = metadata.ContentItemGUID,
            Name = metadata.Name,
            ContentType = DataClassInfoProvider.GetClassName(metadata.ContentTypeID),
            // ContentItemMetadata is language-neutral; system fields don't vary per language.
            LanguageName = string.Empty,
            // A friendly workspace name isn't exposed via the public API at this SDK version (31.5.4) --
            // surfacing the numeric ID lets callers still correlate it with the admin UI.
            WorkspaceName = metadata.WorkspaceId.ToString(),
        };
    }
}
