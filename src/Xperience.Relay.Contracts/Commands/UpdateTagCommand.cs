namespace Xperience.Relay.Contracts.Commands;

/// <summary>Changes a tag's title and/or description. Give at least one of them.</summary>
[RelayCommand("update-tag", Access = RelayAccess.Write)]
public class UpdateTagCommand : IRelayCommand
{
    /// <summary>ID of the tag. Required.</summary>
    public int TagId { get; set; }

    /// <summary>New title. Left as it is when omitted.</summary>
    public string? Title { get; set; }

    /// <summary>New description. Left as it is when omitted; an empty string clears it.</summary>
    public string? Description { get; set; }
}
