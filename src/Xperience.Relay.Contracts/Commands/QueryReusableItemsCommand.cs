namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Queries reusable content items of one or more content types, returning only the requested
/// columns. Results land in <see cref="QueryItemsResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("query-reusable-items")]
public class QueryReusableItemsCommand : QueryItemsCommandBase { }
