namespace Xperience.Relay.Contracts;

/// <summary>
/// A request to run an ordered list of commands against a relay endpoint in a single call.
/// </summary>
public class RelayBatchRequest
{
    public List<RelayCommandEnvelope> Commands { get; set; } = new();
}

/// <summary>
/// The results of a <see cref="RelayBatchRequest"/>, in the same order as the request's commands.
/// </summary>
public class RelayBatchResponse
{
    public List<RelayCommandResult> Results { get; set; } = new();
}
