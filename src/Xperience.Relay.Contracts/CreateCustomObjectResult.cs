namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "create-custom-object" command.</summary>
public class CreateCustomObjectResult
{
    /// <summary>ID of the new object.</summary>
    public int Id { get; set; }
}
