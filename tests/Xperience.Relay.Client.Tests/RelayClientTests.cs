using System.Net;
using System.Text.Json;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;

namespace Xperience.Relay.Client.Tests;

public class RelayClientTests
{
    private static RelayClient BuildClient(StubHttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://relay.example.com/api/relay/") });

    [Fact]
    public async Task ExecuteAsync_PostsEnvelopeWithVerbFromAttribute()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"success":true,"message":"moved 42 to 7"}""");
        var client = BuildClient(handler);

        var result = await client.ExecuteAsync(new MoveWebPageCommand { WebPageId = 42, ParentWebPageId = 7 });

        Assert.True(result.Success);
        Assert.Equal("moved 42 to 7", result.Message);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("https://relay.example.com/api/relay/commands", handler.LastRequest.RequestUri!.ToString());

        using var sentBody = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("move-web-page", sentBody.RootElement.GetProperty("verb").GetString());
        Assert.Equal(42, sentBody.RootElement.GetProperty("parameters").GetProperty("webPageId").GetInt32());
    }

    [Fact]
    public async Task ExecuteAsync_PassesAsThrough()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"success":true}""");
        var client = BuildClient(handler);

        await client.ExecuteAsync(new MoveWebPageCommand { WebPageId = 1, ParentWebPageId = 2 }, @as: "firstMove");

        using var sentBody = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("firstMove", sentBody.RootElement.GetProperty("as").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailResult_WhenResponseBodyIsEmpty()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "null");
        var client = BuildClient(handler);

        var result = await client.ExecuteAsync(new MoveWebPageCommand { WebPageId = 1, ParentWebPageId = 2 });

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailResult_WhenServerReturnsNonSuccessStatus()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.InternalServerError, "Unhandled exception: something broke");
        var client = BuildClient(handler);

        var result = await client.ExecuteAsync(new MoveWebPageCommand { WebPageId = 1, ParentWebPageId = 2 });

        Assert.False(result.Success);
        Assert.Contains("500", result.Error);
        Assert.Contains("Unhandled exception: something broke", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenCommandTypeHasNoRelayCommandAttribute()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "{}");
        var client = BuildClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ExecuteAsync(new UndecoratedCommand()));
    }

    [Fact]
    public async Task ExecuteBatchAsync_PostsCommandsInOrder()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"results":[{"success":true},{"success":true}]}""");
        var client = BuildClient(handler);

        var result = await client.ExecuteBatchAsync(new IRelayCommand[]
        {
            new MoveWebPageCommand { WebPageId = 1, ParentWebPageId = 0 },
            new MoveWebPageCommand { WebPageId = 2, ParentWebPageId = 1 },
        });

        Assert.Equal(2, result.Results.Count);
        Assert.Equal("https://relay.example.com/api/relay/batch", handler.LastRequest!.RequestUri!.ToString());

        using var sentBody = JsonDocument.Parse(handler.LastRequestBody!);
        var commands = sentBody.RootElement.GetProperty("commands");
        Assert.Equal(2, commands.GetArrayLength());
        Assert.Equal(1, commands[0].GetProperty("parameters").GetProperty("webPageId").GetInt32());
        Assert.Equal(2, commands[1].GetProperty("parameters").GetProperty("webPageId").GetInt32());
    }

    [Fact]
    public async Task GetDiscoveryAsync_GetsVerbsEndpoint()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """{"relayVersion":"0.1.0","verbs":[{"verb":"move-web-page","parameters":{}}]}""");
        var client = BuildClient(handler);

        var discovery = await client.GetDiscoveryAsync();

        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal("https://relay.example.com/api/relay/verbs", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("0.1.0", discovery.RelayVersion);
        Assert.Equal("move-web-page", discovery.Verbs.Single().Verb);
    }

    private class UndecoratedCommand : IRelayCommand
    {
    }
}
