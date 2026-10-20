using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Hosting.Tests;

/// <summary>How the endpoints read the envelope they are sent: what may be left out, and how names are matched.</summary>
public class RelayEnvelopeTests : IAsyncLifetime
{
    private const string Key = "admin-key";

    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private GetPageInfoCommand? _pageInfoCommand;
    private int _taxonomyCalls;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddRelayCore(typeof(RelayAccess).Assembly);
        builder.Services.AddRelayHosting();
        builder.Services.AddSingleton<IRelayApiKeyValidator>(new FakeValidator());
        builder.Services.AddScoped<IRelayCommandHandler<GetTaxonomiesCommand>>(_ => new Handler<GetTaxonomiesCommand>(_ => _taxonomyCalls++));
        builder.Services.AddScoped<IRelayCommandHandler<GetPageInfoCommand>>(_ => new Handler<GetPageInfoCommand>(command => _pageInfoCommand = command));

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
    public async Task ACommandWithoutAParametersProperty_RunsWithDefaultValues()
    {
        var response = await PostAsync("""{"verb":"get-taxonomies"}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await response.Content.ReadFromJsonAsync<RelayCommandResult>())!.Success);
        Assert.Equal(1, _taxonomyCalls);
    }

    [Fact]
    public async Task ParametersThatAreNull_AreAFailedResult()
    {
        var response = await PostAsync("""{"verb":"get-taxonomies","parameters":null}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RelayCommandResult>();
        Assert.False(result!.Success);
        Assert.Contains("resolved to null", result.Error);
        Assert.Equal(0, _taxonomyCalls);
    }

    [Fact]
    public async Task ParameterNames_AreMatchedWithoutRegardToCase_AndUnknownOnesAreIgnored()
    {
        var response = await PostAsync("""{"verb":"get-page-info","parameters":{"WebPageId":9,"languageName":"es","noSuchParameter":1}}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(9, _pageInfoCommand!.WebPageId);
        Assert.Equal("es", _pageInfoCommand.LanguageName);
    }

    [Fact]
    public async Task ParametersOfTheWrongType_AreAFailedResultThatNamesTheProperty()
    {
        var response = await PostAsync("""{"verb":"get-page-info","parameters":{"webPageId":"abc"}}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RelayCommandResult>();
        Assert.False(result!.Success);
        Assert.Contains("webPageId", result.Error);
        Assert.Null(_pageInfoCommand);
    }

    [Fact]
    public async Task ABodyThatIsNotJson_IsABadRequest()
    {
        var response = await PostAsync("{");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private Task<HttpResponseMessage> PostAsync(string json)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/relay/commands")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Relay-Api-Key", Key);

        return _client.SendAsync(request);
    }

    private sealed class FakeValidator : IRelayApiKeyValidator
    {
        public Task<RelayApiKeyIdentity?> ValidateAsync(string presentedKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<RelayApiKeyIdentity?>(presentedKey == Key ? new RelayApiKeyIdentity(1, "admin", RelayAccess.Admin) : null);
    }

    private sealed class Handler<TCommand>(Action<TCommand> onHandled) : IRelayCommandHandler<TCommand>
        where TCommand : IRelayCommand
    {
        public Task<RelayCommandResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            onHandled(command);
            return Task.FromResult(RelayCommandResult.Ok("handled"));
        }
    }
}
