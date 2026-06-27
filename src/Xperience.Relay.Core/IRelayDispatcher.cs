using Xperience.Relay.Contracts;

namespace Xperience.Relay.Core;

public interface IRelayDispatcher
{
    Task<RelayCommandResult> DispatchAsync(IRelayCommand command, CancellationToken cancellationToken = default);
}
