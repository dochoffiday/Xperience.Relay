using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class GetTagsCommandHandler(
    IInfoProvider<TaxonomyInfo> taxonomyInfoProvider,
    IInfoProvider<TagInfo> tagInfoProvider) : IRelayCommandHandler<GetTagsCommand>
{
    public Task<RelayCommandResult> HandleAsync(GetTagsCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TaxonomyId is null && command.TaxonomyName is null && command.TagId is null && command.TagName is null)
        {
            return Task.FromResult(RelayCommandResult.Fail("At least one filter (TaxonomyId, TaxonomyName, TagId, or TagName) is required."));
        }

        var query = tagInfoProvider.Get();

        if (command.TagId.HasValue)
        {
            query = query.WhereEquals(nameof(TagInfo.TagID), command.TagId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(command.TagName))
        {
            query = query.WhereEquals(nameof(TagInfo.TagName), command.TagName);
        }
        else if (command.TaxonomyId.HasValue)
        {
            query = query.WhereEquals(nameof(TagInfo.TagTaxonomyID), command.TaxonomyId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(command.TaxonomyName))
        {
            var taxonomy = taxonomyInfoProvider.Get()
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), command.TaxonomyName)
                .FirstOrDefault();

            if (taxonomy is null)
            {
                return Task.FromResult(RelayCommandResult.Fail($"Taxonomy '{command.TaxonomyName}' was not found."));
            }

            query = query.WhereEquals(nameof(TagInfo.TagTaxonomyID), taxonomy.TaxonomyID);
        }

        var tags = query
            .GetEnumerableTypedResult()
            .Select(t => new TagResult
            {
                TagId = t.TagID,
                TagGuid = t.TagGUID,
                TagName = t.TagName,
                TagTitle = t.TagTitle,
                TagDescription = t.TagDescription,
                TaxonomyId = t.TagTaxonomyID,
                ParentTagId = t.TagParentID > 0 ? t.TagParentID : null,
                TagOrder = t.TagOrder
            })
            .ToList();

        return Task.FromResult(RelayCommandResult.Ok(
            message: $"Retrieved {tags.Count} tag(s).",
            data: new GetTagsResult { Tags = tags }));
    }
}
