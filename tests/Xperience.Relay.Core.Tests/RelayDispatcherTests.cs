using Microsoft.Extensions.DependencyInjection;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;

namespace Xperience.Relay.Core.Tests;

public class RelayDispatcherTests
{
    private class FakeMoveHandler : IRelayCommandHandler<MoveWebPageCommand>
    {
        public Task<RelayCommandResult> HandleAsync(MoveWebPageCommand command, CancellationToken cancellationToken = default) =>
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
        services.AddRelayCore(typeof(MoveWebPageCommand).Assembly);
        services.AddScoped<IRelayCommandHandler<MoveWebPageCommand>, FakeMoveHandler>();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task DispatchAsync_RoutesCommandToRegisteredHandler()
    {
        var provider = BuildServices();
        var dispatcher = provider.GetRequiredService<IRelayDispatcher>();

        var result = await dispatcher.DispatchAsync(new MoveWebPageCommand { WebPageId = 42, ParentWebPageId = 7 });

        Assert.True(result.Success);
        Assert.Equal("moved 42 to 7", result.Message);
    }

    [Fact]
    public async Task DispatchAsync_RunsPipelineBehaviorsAroundHandler()
    {
        var behavior = new RecordingBehavior();
        var provider = BuildServices(services => services.AddSingleton<IRelayPipelineBehavior>(behavior));
        var dispatcher = provider.GetRequiredService<IRelayDispatcher>();

        await dispatcher.DispatchAsync(new MoveWebPageCommand { WebPageId = 1, ParentWebPageId = 0 });

        Assert.Equal(new[] { "before", "after" }, behavior.CallOrder);
    }

    [Theory]
    [InlineData("move-web-page", typeof(MoveWebPageCommand))]
    [InlineData("move-content-item", typeof(MoveContentItemCommand))]
    [InlineData("get-page-info", typeof(GetPageInfoCommand))]
    [InlineData("get-page", typeof(GetPageCommand))]
    [InlineData("get-content-info", typeof(GetContentInfoCommand))]
    [InlineData("get-content", typeof(GetContentCommand))]
    [InlineData("get-content-hub-folder", typeof(GetContentHubFolderCommand))]
    [InlineData("create-content-item", typeof(CreateContentItemCommand))]
    [InlineData("create-web-page", typeof(CreateWebPageCommand))]
    [InlineData("query-web-page-items", typeof(QueryWebPageItemsCommand))]
    [InlineData("query-reusable-items", typeof(QueryReusableItemsCommand))]
    [InlineData("update-web-page", typeof(UpdateWebPageCommand))]
    [InlineData("update-content-item", typeof(UpdateContentItemCommand))]
    [InlineData("publish-web-page", typeof(PublishWebPageCommand))]
    [InlineData("unpublish-web-page", typeof(UnpublishWebPageCommand))]
    [InlineData("publish-content-item", typeof(PublishContentItemCommand))]
    [InlineData("unpublish-content-item", typeof(UnpublishContentItemCommand))]
    [InlineData("delete-web-page", typeof(DeleteWebPageCommand))]
    [InlineData("delete-content-item", typeof(DeleteContentItemCommand))]
    [InlineData("update-slug", typeof(UpdateSlugCommand))]
    [InlineData("reoptimize-asset", typeof(ReoptimizeAssetCommand))]
    [InlineData("rename-asset", typeof(RenameAssetCommand))]
    [InlineData("query-sql", typeof(QuerySqlCommand))]
    public void RelayVerbRegistry_DiscoversCommandsFromContractsAssembly(string verb, Type expectedType)
    {
        var registry = new RelayVerbRegistry(new[] { typeof(MoveWebPageCommand).Assembly });

        Assert.Contains(verb, registry.Verbs, StringComparer.OrdinalIgnoreCase);
        Assert.True(registry.TryGetCommandType(verb, out var commandType));
        Assert.Equal(expectedType, commandType);
    }
}
