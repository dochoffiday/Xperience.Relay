using Xperience.Relay.Contracts;

namespace Xperience.Relay.Core;

public class RelayDispatcher : IRelayDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyList<IRelayPipelineBehavior> _behaviors;

    public RelayDispatcher(IServiceProvider serviceProvider, IEnumerable<IRelayPipelineBehavior> behaviors)
    {
        _serviceProvider = serviceProvider;
        _behaviors = behaviors.ToList();
    }

    public async Task<RelayCommandResult> DispatchAsync(IRelayCommand command, CancellationToken cancellationToken = default)
    {
        RelayNext next = () => InvokeHandlerAsync(command, cancellationToken);

        for (var i = _behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = _behaviors[i];
            var previousNext = next;
            next = () => behavior.HandleAsync(command, previousNext, cancellationToken);
        }

        return await next();
    }

    private Task<RelayCommandResult> InvokeHandlerAsync(IRelayCommand command, CancellationToken cancellationToken)
    {
        var handlerType = typeof(IRelayCommandHandler<>).MakeGenericType(command.GetType());
        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No {handlerType.Name} registered for command type '{command.GetType().Name}'.");

        var handleMethod = handlerType.GetMethod(nameof(IRelayCommandHandler<IRelayCommand>.HandleAsync))!;
        return (Task<RelayCommandResult>)handleMethod.Invoke(handler, new object[] { command, cancellationToken })!;
    }
}
