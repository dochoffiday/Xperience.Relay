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
