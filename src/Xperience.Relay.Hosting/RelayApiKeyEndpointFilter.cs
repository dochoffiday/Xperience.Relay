using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Core;

namespace Xperience.Relay.Hosting;

/// <summary>
/// Authenticates every relay request by validating the key in <see cref="RelayHostingOptions.ApiKeyHeaderName"/>
/// through <see cref="IRelayApiKeyValidator"/>, then checks that the key's access tier covers every verb in the
/// request. A batch is authorized as a whole before any command in it runs.
/// </summary>
public class RelayApiKeyEndpointFilter(
    IOptions<RelayHostingOptions> options,
    IRelayApiKeyValidator validator,
    RelayVerbRegistry registry,
    ILogger<RelayApiKeyEndpointFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var providedKey = httpContext.Request.Headers[options.Value.ApiKeyHeaderName].ToString();

        var identity = providedKey.Length == 0
            ? null
            : await validator.ValidateAsync(providedKey, httpContext.RequestAborted);

        if (identity is null)
        {
            logger.LogWarning("Relay request rejected: missing or invalid API key.");
            return Results.Unauthorized();
        }

        foreach (var verb in GetRequestedVerbs(context))
        {
            if (registry.TryGetRequiredAccess(verb, out var required) && identity.Access < required)
            {
                logger.LogWarning(
                    "Relay API key '{KeyName}' ({Access}) was denied verb '{Verb}', which requires {Required}.",
                    identity.Name, identity.Access, verb, required);

                return Results.Json(
                    RelayCommandResult.Fail($"This API key has {identity.Access} access, but '{verb}' requires {required}."),
                    statusCode: StatusCodes.Status403Forbidden);
            }
        }

        using (logger.BeginScope(new Dictionary<string, object> { ["RelayApiKey"] = identity.Name }))
        {
            return await next(context);
        }
    }

    private static IEnumerable<string> GetRequestedVerbs(EndpointFilterInvocationContext context)
    {
        foreach (var argument in context.Arguments)
        {
            switch (argument)
            {
                case RelayCommandEnvelope envelope:
                    yield return envelope.Verb;
                    break;
                case RelayBatchRequest batch:
                    foreach (var item in batch.Commands)
                    {
                        yield return item.Verb;
                    }

                    break;
            }
        }
    }
}
