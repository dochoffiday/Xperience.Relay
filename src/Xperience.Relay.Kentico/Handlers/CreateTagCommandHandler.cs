using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class CreateTagCommandHandler(
    IInfoProvider<TaxonomyInfo> taxonomyInfoProvider,
    IInfoProvider<TagInfo> tagInfoProvider) : IRelayCommandHandler<CreateTagCommand>
{
    public async Task<RelayCommandResult> HandleAsync(CreateTagCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return RelayCommandResult.Fail("Title is required.");
        }

        if (command.TaxonomyId is null && string.IsNullOrWhiteSpace(command.TaxonomyName))
        {
            return RelayCommandResult.Fail("Either TaxonomyId or TaxonomyName is required.");
        }

        TaxonomyInfo? taxonomy = null;

        if (command.TaxonomyId.HasValue)
        {
            taxonomy = await taxonomyInfoProvider.GetAsync(command.TaxonomyId.Value, cancellationToken);
        }
        else
        {
            taxonomy = taxonomyInfoProvider.Get()
                .WhereEquals(nameof(TaxonomyInfo.TaxonomyName), command.TaxonomyName)
                .FirstOrDefault();
        }

        if (taxonomy is null)
        {
            return RelayCommandResult.Fail($"Taxonomy was not found.");
        }

        var codeName = string.IsNullOrWhiteSpace(command.CodeName)
            ? ToCodeName(command.Title)
            : command.CodeName;

        var tag = new TagInfo
        {
            TagTaxonomyID = taxonomy.TaxonomyID,
            TagName = codeName,
            TagTitle = command.Title,
            TagDescription = command.Description ?? string.Empty
        };

        if (command.ParentTagId is > 0)
        {
            tag.TagParentID = command.ParentTagId.Value;
        }

        if (command.Order.HasValue)
        {
            tag.TagOrder = command.Order.Value;
        }

        await tagInfoProvider.SetAsync(tag, cancellationToken);

        return RelayCommandResult.Ok(
            message: $"Created tag '{command.Title}' (ID={tag.TagID}) in taxonomy '{taxonomy.TaxonomyName}'.",
            data: new CreateTagResult
            {
                TagId = tag.TagID,
                TagGuid = tag.TagGUID,
                TagName = tag.TagName,
                TagTitle = tag.TagTitle
            });
    }

    private static string ToCodeName(string title) =>
        System.Text.RegularExpressions.Regex.Replace(title.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-").Trim('-');
}
