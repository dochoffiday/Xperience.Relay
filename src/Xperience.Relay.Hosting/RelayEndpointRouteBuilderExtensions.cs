using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Core;

namespace Xperience.Relay.Hosting;

public static class RelayEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the relay's three HTTP endpoints under <see cref="RelayHostingOptions.BasePath"/>:
    /// POST {basePath}/commands (single command), POST {basePath}/batch (ordered list of commands),
    /// and GET {basePath}/verbs (discovery). All three require <see cref="RelayApiKeyEndpointFilter"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapRelayEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var basePath = endpoints.ServiceProvider.GetRequiredService<IOptions<RelayHostingOptions>>().Value.BasePath;
        var group = endpoints.MapGroup(basePath).AddEndpointFilter<RelayApiKeyEndpointFilter>();

        group.MapPost("/commands", ExecuteCommandAsync);
        group.MapPost("/batch", ExecuteBatchAsync);
        group.MapGet("/verbs", GetDiscovery);

        return endpoints;
    }

    private static async Task<IResult> ExecuteCommandAsync(
        RelayCommandEnvelope envelope,
        IRelayDispatcher dispatcher,
        RelayVerbRegistry registry,
        CancellationToken cancellationToken)
    {
        var result = await ExecuteEnvelopeAsync(envelope, dispatcher, registry, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ExecuteBatchAsync(
        RelayBatchRequest request,
        IRelayDispatcher dispatcher,
        RelayVerbRegistry registry,
        CancellationToken cancellationToken)
    {
        var results = new List<RelayCommandResult>(request.Commands.Count);
        foreach (var envelope in request.Commands)
        {
            results.Add(await ExecuteEnvelopeAsync(envelope, dispatcher, registry, cancellationToken));
        }

        return Results.Ok(new RelayBatchResponse { Results = results });
    }

    private static async Task<RelayCommandResult> ExecuteEnvelopeAsync(
        RelayCommandEnvelope envelope,
        IRelayDispatcher dispatcher,
        RelayVerbRegistry registry,
        CancellationToken cancellationToken)
    {
        if (!registry.TryGetCommandType(envelope.Verb, out var commandType))
        {
            return RelayCommandResult.Fail($"Unknown verb '{envelope.Verb}'.");
        }

        IRelayCommand? command;
        try
        {
            command = (IRelayCommand?)envelope.Parameters.Deserialize(commandType);
        }
        catch (JsonException ex)
        {
            return RelayCommandResult.Fail($"Could not deserialize parameters for verb '{envelope.Verb}': {ex.Message}");
        }

        if (command is null)
        {
            return RelayCommandResult.Fail($"Parameters for verb '{envelope.Verb}' resolved to null.");
        }

        return await dispatcher.DispatchAsync(command, cancellationToken);
    }

    private static IResult GetDiscovery(RelayVerbRegistry registry)
    {
        var verbs = registry.Verbs.Select(verb =>
        {
            registry.TryGetCommandType(verb, out var commandType);
            var parameters = commandType!.GetProperties()
                .ToDictionary(p => p.Name, p => p.PropertyType.Name);
            return new RelayVerbDescriptor { Verb = verb, Parameters = parameters };
        }).ToList();

        var relayVersion = typeof(RelayVerbRegistry).Assembly.GetName().Version?.ToString() ?? "unknown";
        return Results.Ok(new RelayDiscoveryResponse { RelayVersion = relayVersion, Verbs = verbs });
    }
}
