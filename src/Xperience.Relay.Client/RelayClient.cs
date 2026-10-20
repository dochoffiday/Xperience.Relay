using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xperience.Relay.Contracts;

namespace Xperience.Relay.Client;

/// <summary>
/// Calls a deployed Xperience.Relay endpoint over HTTP. No dependency on the Kentico SDK --
/// just <see cref="HttpClient"/> and <see cref="Xperience.Relay.Contracts"/> types, so it's safe to
/// reference from a remote caller that has no business knowing about Kentico's APIs.
/// </summary>
public class RelayClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private static readonly ConcurrentDictionary<Type, string> VerbsByCommandType = new();

    private readonly HttpClient _httpClient;

    public RelayClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>Sends one command to the relay and returns its result.</summary>
    /// <param name="command">Any command type from <c>Xperience.Relay.Contracts</c>; its verb comes from its <c>[RelayCommand]</c> attribute.</param>
    /// <param name="as">Reserved (see <see cref="RelayCommandEnvelope.As"/>); the relay doesn't use it yet.</param>
    /// <param name="cancellationToken">Cancels the HTTP request.</param>
    /// <returns>
    /// The relay's result. A non-2xx HTTP response (401 invalid API key, 403 key's tier too low, ...) doesn't throw:
    /// it comes back as a failed result whose message holds the status code and body.
    /// </returns>
    public async Task<RelayCommandResult> ExecuteAsync(
        IRelayCommand command,
        string? @as = null,
        CancellationToken cancellationToken = default)
    {
        var envelope = ToEnvelope(command, @as);
        using var response = await _httpClient.PostAsJsonAsync("commands", envelope, JsonOptions, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadBodyAsync(response.Content, cancellationToken).ConfigureAwait(false);
            return RelayCommandResult.Fail($"Relay endpoint returned {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<RelayCommandResult>(JsonOptions, cancellationToken).ConfigureAwait(false);
        return result ?? RelayCommandResult.Fail("Relay endpoint returned an empty response.");
    }

    /// <summary>
    /// Sends several commands in one request. They run in order and one failing doesn't stop the rest: the
    /// response has one result per command, in the same order. The batch is authorized as a whole, so the API
    /// key's tier must cover the most privileged command in it.
    /// </summary>
    /// <exception cref="HttpRequestException">The relay answered with a non-2xx status (e.g. 401 or 403), which rejects the whole batch.</exception>
    public async Task<RelayBatchResponse> ExecuteBatchAsync(
        IEnumerable<IRelayCommand> commands,
        CancellationToken cancellationToken = default)
    {
        var request = new RelayBatchRequest
        {
            Commands = commands.Select(command => ToEnvelope(command, @as: null)).ToList(),
        };

        using var response = await _httpClient.PostAsJsonAsync("batch", request, JsonOptions, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadBodyAsync(response.Content, cancellationToken).ConfigureAwait(false);
            var message = $"Relay batch endpoint returned {(int)response.StatusCode}: {body}";
#if NET5_0_OR_GREATER
            throw new HttpRequestException(message, null, response.StatusCode);
#else
            throw new HttpRequestException(message);
#endif
        }

        var result = await response.Content.ReadFromJsonAsync<RelayBatchResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
        return result ?? new RelayBatchResponse();
    }

    /// <summary>
    /// Lists the verbs the relay accepts, each with its parameter names and their CLR type names, plus the relay's
    /// version, as "GET /verbs" returns them. Useful to check that a deployed relay supports a verb before using it.
    /// </summary>
    public async Task<RelayDiscoveryResponse> GetDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<RelayDiscoveryResponse>("verbs", JsonOptions, cancellationToken).ConfigureAwait(false);
        return result ?? new RelayDiscoveryResponse();
    }

    private static Task<string> ReadBodyAsync(HttpContent content, CancellationToken cancellationToken) =>
#if NET5_0_OR_GREATER
        content.ReadAsStringAsync(cancellationToken);
#else
        content.ReadAsStringAsync();
#endif

    private static RelayCommandEnvelope ToEnvelope(IRelayCommand command, string? @as)
    {
        var commandType = command.GetType();
        var parameters = JsonSerializer.SerializeToElement(command, commandType, JsonOptions);
        return new RelayCommandEnvelope { Verb = GetVerb(commandType), As = @as, Parameters = parameters };
    }

    private static string GetVerb(Type commandType) =>
        VerbsByCommandType.GetOrAdd(commandType, type =>
        {
            var attribute = (RelayCommandAttribute?)Attribute.GetCustomAttribute(type, typeof(RelayCommandAttribute));
            if (attribute is null)
            {
                throw new InvalidOperationException(
                    $"{type.Name} has no [RelayCommand] attribute, so RelayClient doesn't know which verb to send it as.");
            }

            return attribute.Verb;
        });
}
