using Xperience.Relay.Contracts;

namespace Xperience.Relay.Core;

/// <summary>
/// Executes a specific kind of <see cref="IRelayCommand"/>. Implementations for Kentico-specific
/// commands (move, sort, query, ...) live in Xperience.Relay.Kentico.
/// </summary>
public interface IRelayCommandHandler<in TCommand> where TCommand : IRelayCommand
{
    Task<RelayCommandResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
