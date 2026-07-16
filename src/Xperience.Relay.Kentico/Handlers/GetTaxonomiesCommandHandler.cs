using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class GetTaxonomiesCommandHandler(
    IInfoProvider<TaxonomyInfo> taxonomyInfoProvider) : IRelayCommandHandler<GetTaxonomiesCommand>
{
    public Task<RelayCommandResult> HandleAsync(GetTaxonomiesCommand command, CancellationToken cancellationToken = default)
    {
        var taxonomies = taxonomyInfoProvider.Get()
            .GetEnumerableTypedResult()
            .Select(t => new TaxonomyResult
            {
                TaxonomyId = t.TaxonomyID,
                TaxonomyGuid = t.TaxonomyGUID,
                TaxonomyName = t.TaxonomyName,
                TaxonomyTitle = t.TaxonomyTitle,
                TaxonomyDescription = t.TaxonomyDescription
            })
            .ToList();

        return Task.FromResult(RelayCommandResult.Ok(
            message: $"Retrieved {taxonomies.Count} taxonomy/taxonomies.",
            data: new GetTaxonomiesResult { Taxonomies = taxonomies }));
    }
}
