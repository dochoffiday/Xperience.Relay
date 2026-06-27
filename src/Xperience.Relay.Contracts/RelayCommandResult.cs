namespace Xperience.Relay.Contracts;

/// <summary>
/// The outcome of executing a single <see cref="IRelayCommand"/>.
/// </summary>
public class RelayCommandResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }

    /// <summary>Verb-specific payload (e.g. a <see cref="WebPageInfo"/> for "get-page-info"). Travels as JSON.</summary>
    public object? Data { get; set; }

    public static RelayCommandResult Ok(string? message = null, object? data = null) =>
        new() { Success = true, Message = message, Data = data };

    public static RelayCommandResult Fail(string error) => new() { Success = false, Error = error };
}
