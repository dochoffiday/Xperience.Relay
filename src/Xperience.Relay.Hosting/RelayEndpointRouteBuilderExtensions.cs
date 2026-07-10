using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xperience.Relay.Contracts;
using Xperience.Relay.Core;

namespace Xperience.Relay.Hosting;

public static class RelayEndpointRouteBuilderExtensions
{
    private static readonly JsonSerializerOptions DeserializeOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Maps the relay's three HTTP endpoints under <see cref="RelayHostingOptions.BasePath"/>:
    /// POST {basePath}/commands (single command), POST {basePath}/batch (ordered list of commands),
    /// and GET {basePath}/verbs (discovery). All three require <see cref="RelayApiKeyEndpointFilter"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapRelayEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var hostingOptions = endpoints.ServiceProvider.GetRequiredService<IOptions<RelayHostingOptions>>().Value;
        var group = endpoints.MapGroup(hostingOptions.BasePath)
            .AddEndpointFilter<RelayApiKeyEndpointFilter>()
            .AddEndpointFilter(new RelayRequestBodySizeFilter(hostingOptions.MaxRequestBodySizeBytes));

        group.MapPost("/commands", ExecuteCommandAsync);
        group.MapPost("/batch", ExecuteBatchAsync);
        group.MapGet("/verbs", GetDiscovery);

        return endpoints;
    }

    private static async Task<IResult> ExecuteCommandAsync(
        RelayCommandEnvelope envelope,
        IRelayDispatcher dispatcher,
        RelayVerbRegistry registry,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var result = await ExecuteEnvelopeAsync(envelope, dispatcher, registry, loggerFactory.CreateLogger("Xperience.Relay.Hosting"), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ExecuteBatchAsync(
        RelayBatchRequest request,
        IRelayDispatcher dispatcher,
        RelayVerbRegistry registry,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("Xperience.Relay.Hosting");
        var results = new List<RelayCommandResult>(request.Commands.Count);
        foreach (var envelope in request.Commands)
        {
            results.Add(await ExecuteEnvelopeAsync(envelope, dispatcher, registry, logger, cancellationToken));
        }

        return Results.Ok(new RelayBatchResponse { Results = results });
    }

    private static async Task<RelayCommandResult> ExecuteEnvelopeAsync(
        RelayCommandEnvelope envelope,
        IRelayDispatcher dispatcher,
        RelayVerbRegistry registry,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (!registry.TryGetCommandType(envelope.Verb, out var commandType))
        {
            return RelayCommandResult.Fail($"Unknown verb '{envelope.Verb}'.");
        }

        IRelayCommand? command;
        try
        {
            command = (IRelayCommand?)envelope.Parameters.Deserialize(commandType, DeserializeOptions);
        }
        catch (JsonException ex)
        {
            return RelayCommandResult.Fail($"Could not deserialize parameters for verb '{envelope.Verb}': {ex.Message}");
        }

        if (command is null)
        {
            return RelayCommandResult.Fail($"Parameters for verb '{envelope.Verb}' resolved to null.");
        }

        try
        {
            return await dispatcher.DispatchAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception executing verb '{Verb}'", envelope.Verb);
            return RelayCommandResult.Fail($"Unhandled exception executing verb '{envelope.Verb}': [{ex.GetType().Name}] {ex.Message}");
        }
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
