using Xperience.Relay.Contracts;

namespace Xperience.Relay.Core;

public delegate Task<RelayCommandResult> RelayNext();

/// <summary>
/// Wraps command execution for cross-cutting concerns (logging, validation, retry, auth).
/// Behaviors run in registration order, each deciding whether to call the <see cref="RelayNext"/> it is given.
/// </summary>
public interface IRelayPipelineBehavior
{
    /// <param name="command">The command being dispatched.</param>
    /// <param name="next">Runs the rest of the pipeline and then the command's handler. A behavior that doesn't call it stops the command and returns a result of its own.</param>
    /// <param name="cancellationToken">Cancelled when the request is aborted.</param>
    Task<RelayCommandResult> HandleAsync(IRelayCommand command, RelayNext next, CancellationToken cancellationToken = default);
}
