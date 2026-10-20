using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Hosting.Tests;

public class RelayAuthorizationTests : IAsyncLifetime
{
    private const string Header = "X-Relay-Api-Key";
    private const string ReadKey = "read-key";
    private const string WriteKey = "write-key";
    private const string AdminKey = "admin-key";

    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private int _readCalls;
    private int _writeCalls;
    private int _adminCalls;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddRelayCore(typeof(RelayAccess).Assembly);
        builder.Services.AddRelayHosting();
        builder.Services.AddSingleton<IRelayApiKeyValidator>(new FakeValidator(new Dictionary<string, RelayApiKeyIdentity>
        {
            [ReadKey] = new(1, "reader", RelayAccess.Read),
            [WriteKey] = new(2, "writer", RelayAccess.Write),
            [AdminKey] = new(3, "admin", RelayAccess.Admin),
        }));
        builder.Services.AddScoped<IRelayCommandHandler<GetPageInfoCommand>>(_ => new CountingHandler<GetPageInfoCommand>(() => _readCalls++));
        builder.Services.AddScoped<IRelayCommandHandler<MoveWebPageCommand>>(_ => new CountingHandler<MoveWebPageCommand>(() => _writeCalls++));
        builder.Services.AddScoped<IRelayCommandHandler<ExecuteSqlCommand>>(_ => new CountingHandler<ExecuteSqlCommand>(() => _adminCalls++));

        _app = builder.Build();
        _app.MapRelayEndpoints();
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task MissingKey_IsUnauthorized()
    {
        var response = await PostCommandAsync(null, "get-page-info");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, _readCalls);
    }

    [Fact]
    public async Task UnknownKey_IsUnauthorized()
    {
        var response = await PostCommandAsync("not-a-real-key", "get-page-info");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, _readCalls);
    }

    [Theory]
    [InlineData(ReadKey, "get-page-info")]
    [InlineData(WriteKey, "get-page-info")]
    [InlineData(WriteKey, "move-web-page")]
    [InlineData(AdminKey, "get-page-info")]
    [InlineData(AdminKey, "move-web-page")]
    [InlineData(AdminKey, "execute-sql")]
    public async Task KeyAtOrAboveTheVerbsTier_Succeeds(string key, string verb)
    {
        var response = await PostCommandAsync(key, verb);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RelayCommandResult>();
        Assert.True(result!.Success);
    }

    [Theory]
    [InlineData(ReadKey, "move-web-page")]
    [InlineData(ReadKey, "execute-sql")]
    [InlineData(WriteKey, "execute-sql")]
    [InlineData(ReadKey, "Execute-SQL")]
    public async Task KeyBelowTheVerbsTier_IsForbiddenAndHandlerNeverRuns(string key, string verb)
    {
        var response = await PostCommandAsync(key, verb);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, _writeCalls + _adminCalls);
    }

    [Fact]
    public async Task Forbidden_ExplainsWhichTierIsNeeded()
    {
        var response = await PostCommandAsync(ReadKey, "execute-sql");

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Read", body);
        Assert.Contains("Admin", body);
        Assert.Contains("execute-sql", body);
    }

    [Fact]
    public async Task Batch_WithAnyForbiddenVerb_RunsNothing()
    {
        var response = await PostBatchAsync(WriteKey, "get-page-info", "move-web-page", "execute-sql");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, _readCalls + _writeCalls + _adminCalls);
    }

    [Fact]
    public async Task Batch_WithinTheKeysTier_RunsEveryCommand()
    {
        var response = await PostBatchAsync(WriteKey, "get-page-info", "move-web-page");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, _readCalls);
        Assert.Equal(1, _writeCalls);
    }

    [Fact]
    public async Task UnknownVerb_WithValidKey_ReturnsAFailedResultNotForbidden()
    {
        var response = await PostCommandAsync(ReadKey, "no-such-verb");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RelayCommandResult>();
        Assert.False(result!.Success);
    }

    [Fact]
    public async Task Discovery_RequiresAValidKeyButNoParticularTier()
    {
        using var anonymous = new HttpRequestMessage(HttpMethod.Get, "/api/relay/verbs");
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.SendAsync(anonymous)).StatusCode);

        using var authenticated = new HttpRequestMessage(HttpMethod.Get, "/api/relay/verbs");
        authenticated.Headers.Add(Header, ReadKey);
        Assert.Equal(HttpStatusCode.OK, (await _client.SendAsync(authenticated)).StatusCode);
    }

    private Task<HttpResponseMessage> PostCommandAsync(string? key, string verb)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/relay/commands")
        {
            Content = JsonContent.Create(new { verb, parameters = new { } }),
        };
        if (key is not null)
        {
            request.Headers.Add(Header, key);
        }

        return _client.SendAsync(request);
    }

    private Task<HttpResponseMessage> PostBatchAsync(string key, params string[] verbs)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/relay/batch")
        {
            Content = JsonContent.Create(new { commands = verbs.Select(verb => new { verb, parameters = new { } }) }),
        };
        request.Headers.Add(Header, key);

        return _client.SendAsync(request);
    }

    private sealed class FakeValidator(Dictionary<string, RelayApiKeyIdentity> keys) : IRelayApiKeyValidator
    {
        public Task<RelayApiKeyIdentity?> ValidateAsync(string presentedKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(keys.GetValueOrDefault(presentedKey));
    }

    private sealed class CountingHandler<TCommand>(Action onHandled) : IRelayCommandHandler<TCommand>
        where TCommand : IRelayCommand
    {
        public Task<RelayCommandResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            onHandled();
            return Task.FromResult(RelayCommandResult.Ok("handled"));
        }
    }
}
