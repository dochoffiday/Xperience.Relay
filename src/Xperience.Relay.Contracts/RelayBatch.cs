namespace Xperience.Relay.Contracts;

/// <summary>
/// A request to run an ordered list of commands against a relay endpoint in a single call.
/// </summary>
public class RelayBatchRequest
{
    /// <summary>The commands to run, in order.</summary>
    public List<RelayCommandEnvelope> Commands { get; set; } = new();
}

/// <summary>
/// The results of a <see cref="RelayBatchRequest"/>, in the same order as the request's commands.
/// </summary>
public class RelayBatchResponse
{
    /// <summary>One result per command of the request, in the same order. A command that fails doesn't stop the ones after it.</summary>
    public List<RelayCommandResult> Results { get; set; } = new();
}
