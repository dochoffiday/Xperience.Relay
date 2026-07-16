using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class CreateTaxonomyCommandHandler(
    IInfoProvider<TaxonomyInfo> taxonomyInfoProvider) : IRelayCommandHandler<CreateTaxonomyCommand>
{
    public async Task<RelayCommandResult> HandleAsync(CreateTaxonomyCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return RelayCommandResult.Fail("Title is required.");
        }

        var codeName = string.IsNullOrWhiteSpace(command.CodeName)
            ? ToCodeName(command.Title)
            : command.CodeName;

        var taxonomy = new TaxonomyInfo
        {
            TaxonomyName = codeName,
            TaxonomyTitle = command.Title,
            TaxonomyDescription = command.Description ?? string.Empty
        };

        await taxonomyInfoProvider.SetAsync(taxonomy, cancellationToken);

        return RelayCommandResult.Ok(
            message: $"Created taxonomy '{command.Title}' (ID={taxonomy.TaxonomyID}).",
            data: new CreateTaxonomyResult
            {
                TaxonomyId = taxonomy.TaxonomyID,
                TaxonomyGuid = taxonomy.TaxonomyGUID,
                TaxonomyName = taxonomy.TaxonomyName,
                TaxonomyTitle = taxonomy.TaxonomyTitle
            });
    }

    private static string ToCodeName(string title) =>
        System.Text.RegularExpressions.Regex.Replace(title.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-").Trim('-');
}
