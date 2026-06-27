using Xperience.Relay.Contracts;

namespace Xperience.Relay.Core;

public delegate Task<RelayCommandResult> RelayNext();

/// <summary>
/// Wraps command execution for cross-cutting concerns (logging, validation, retry, auth).
/// Behaviors run in registration order, each deciding whether to call <paramref name="next"/>.
/// </summary>
public interface IRelayPipelineBehavior
{
    Task<RelayCommandResult> HandleAsync(IRelayCommand command, RelayNext next, CancellationToken cancellationToken = default);
}
