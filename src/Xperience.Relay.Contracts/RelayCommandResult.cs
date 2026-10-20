using System.Text.Json;

namespace Xperience.Relay.Contracts;

/// <summary>
/// The outcome of executing a single <see cref="IRelayCommand"/>.
/// </summary>
public class RelayCommandResult
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>True when the command was carried out. False when it wasn't; <see cref="Error"/> says why.</summary>
    public bool Success { get; set; }

    /// <summary>A sentence about what was done, for commands that report one. Null when the command failed.</summary>
    public string? Message { get; set; }

    /// <summary>Why the command couldn't be carried out. Null when it succeeded.</summary>
    public string? Error { get; set; }

    /// <summary>
    /// Verb-specific payload (e.g. a <see cref="WebPageInfo"/> for "get-page-info"). Null when the command failed or has
    /// nothing to return. Travels as JSON; <see cref="GetData{T}"/> reads it as the type the command returns.
    /// </summary>
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
