using Microsoft.Extensions.DependencyInjection;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Core.Tests;

public class RelayDispatcherTests
{
    private class FakeMoveHandler : IRelayCommandHandler<MoveCommand>
    {
        public Task<RelayCommandResult> HandleAsync(MoveCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(RelayCommandResult.Ok($"moved {command.WebPageId} to {command.ParentWebPageId}"));
    }

    private class RecordingBehavior : IRelayPipelineBehavior
    {
        public List<string> CallOrder { get; } = new();

        public async Task<RelayCommandResult> HandleAsync(IRelayCommand command, RelayNext next, CancellationToken cancellationToken = default)
        {
            CallOrder.Add("before");
            var result = await next();
            CallOrder.Add("after");
            return result;
        }
    }

    private static IServiceProvider BuildServices(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddRelayCore(typeof(MoveCommand).Assembly);
        services.AddScoped<IRelayCommandHandler<MoveCommand>, FakeMoveHandler>();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task DispatchAsync_RoutesCommandToRegisteredHandler()
    {
        var provider = BuildServices();
        var dispatcher = provider.GetRequiredService<IRelayDispatcher>();

        var result = await dispatcher.DispatchAsync(new MoveCommand { WebPageId = 42, ParentWebPageId = 7 });

        Assert.True(result.Success);
        Assert.Equal("moved 42 to 7", result.Message);
    }

    [Fact]
    public async Task DispatchAsync_RunsPipelineBehaviorsAroundHandler()
    {
        var behavior = new RecordingBehavior();
        var provider = BuildServices(services => services.AddSingleton<IRelayPipelineBehavior>(behavior));
        var dispatcher = provider.GetRequiredService<IRelayDispatcher>();

        await dispatcher.DispatchAsync(new MoveCommand { WebPageId = 1, ParentWebPageId = 0 });

        Assert.Equal(new[] { "before", "after" }, behavior.CallOrder);
    }

    [Theory]
    [InlineData("move", typeof(MoveCommand))]
    [InlineData("get-page-info", typeof(GetPageInfoCommand))]
    [InlineData("get-page", typeof(GetPageCommand))]
    [InlineData("get-content-info", typeof(GetContentInfoCommand))]
    [InlineData("get-content", typeof(GetContentCommand))]
    public void RelayVerbRegistry_DiscoversCommandsFromContractsAssembly(string verb, Type expectedType)
    {
        var registry = new RelayVerbRegistry(new[] { typeof(MoveCommand).Assembly });

        Assert.Contains(verb, registry.Verbs, StringComparer.OrdinalIgnoreCase);
        Assert.True(registry.TryGetCommandType(verb, out var commandType));
        Assert.Equal(expectedType, commandType);
    }
}
