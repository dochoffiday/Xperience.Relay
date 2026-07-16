using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class UpdateTaxonomyCommandHandler(
    IInfoProvider<TaxonomyInfo> taxonomyInfoProvider) : IRelayCommandHandler<UpdateTaxonomyCommand>
{
    public async Task<RelayCommandResult> HandleAsync(UpdateTaxonomyCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Title is null && command.Description is null)
        {
            return RelayCommandResult.Fail("At least one of Title or Description must be provided.");
        }

        var taxonomy = await taxonomyInfoProvider.GetAsync(command.TaxonomyId, cancellationToken);

        if (taxonomy is null)
        {
            return RelayCommandResult.Fail($"Taxonomy {command.TaxonomyId} was not found.");
        }

        if (command.Title is not null)
        {
            taxonomy.TaxonomyTitle = command.Title;
        }

        if (command.Description is not null)
        {
            taxonomy.TaxonomyDescription = command.Description;
        }

        await taxonomyInfoProvider.SetAsync(taxonomy, cancellationToken);

        return RelayCommandResult.Ok($"Updated taxonomy {command.TaxonomyId}.");
    }
}
