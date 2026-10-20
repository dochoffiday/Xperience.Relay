namespace Xperience.Relay.Contracts.Commands;

/// <summary>Lists all taxonomies. Returns a <see cref="GetTaxonomiesResult"/> in <see cref="RelayCommandResult.Data"/>.</summary>
[RelayCommand("get-taxonomies", Access = RelayAccess.Read)]
public class GetTaxonomiesCommand : IRelayCommand { }
