using System.Text.Json;

namespace Xperience.Relay.Contracts;

/// <summary>
/// The outcome of executing a single <see cref="IRelayCommand"/>.
/// </summary>
public class RelayCommandResult
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }

    /// <summary>Verb-specific payload (e.g. a <see cref="WebPageInfo"/> for "get-page-info"). Travels as JSON.</summary>
    public object? Data { get; set; }

    /// <summary>
    /// Deserializes <see cref="Data"/> into <typeparamref name="T"/>. Returns null if
    /// <see cref="Data"/> is null or cannot be deserialized to the requested type.
    /// </summary>
    public T? GetData<T>()
    {
        if (Data is T direct) return direct;
        if (Data is JsonElement element) return element.Deserialize<T>(JsonOptions);
        if (Data != null)
        {
            var json = JsonSerializer.Serialize(Data, JsonOptions);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        return default;
    }

    public static RelayCommandResult Ok(string? message = null, object? data = null) =>
        new() { Success = true, Message = message, Data = data };

    public static RelayCommandResult Fail(string error) => new() { Success = false, Error = error };
}
