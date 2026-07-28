using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class DeleteTaxonomyCommandHandler(
    IInfoProvider<TaxonomyInfo> taxonomyInfoProvider,
    ITaxonomyManager taxonomyManager) : IRelayCommandHandler<DeleteTaxonomyCommand>
{
    public async Task<RelayCommandResult> HandleAsync(DeleteTaxonomyCommand command, CancellationToken cancellationToken = default)
    {
        var taxonomy = await taxonomyInfoProvider.GetAsync(command.TaxonomyId, cancellationToken);

        if (taxonomy is null)
        {
            return RelayCommandResult.Fail($"Taxonomy {command.TaxonomyId} was not found.");
        }

        await taxonomyManager.DeleteTaxonomy(command.TaxonomyId, cancellationToken);

        return RelayCommandResult.Ok($"Deleted taxonomy {command.TaxonomyId}.");
    }
}
