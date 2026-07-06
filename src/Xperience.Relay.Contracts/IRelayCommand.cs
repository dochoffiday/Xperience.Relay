namespace Xperience.Relay.Contracts;

/// <summary>
/// Marker interface for a command that can be dispatched through Xperience.Relay.
/// Concrete commands (e.g. MoveWebPageCommand) carry only the parameters needed to perform
/// the action -- they don't know how they're executed or transported.
/// </summary>
public interface IRelayCommand
{
}
