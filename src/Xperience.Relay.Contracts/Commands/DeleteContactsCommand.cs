namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Bulk-deletes the contacts that match a SQL condition, through Kentico's contact deletion service. This cannot be
/// undone: it also removes the contacts' consent agreements, activities and email recipients.
/// </summary>
[RelayCommand("delete-contacts", Access = RelayAccess.Admin)]
public class DeleteContactsCommand : IRelayCommand
{
    /// <summary>
    /// A SQL WHERE expression, without the <c>WHERE</c> keyword, over the contact table's columns, e.g.
    /// <c>ContactEmail IS NULL</c>. Required.
    /// </summary>
    public string WhereClause { get; set; } = string.Empty;

    /// <summary>
    /// When omitted (or 0), Kentico deletes every matching contact, in batches of 1000. When given, only a single batch
    /// of this size is deleted per call.
    /// </summary>
    public int? BatchSize { get; set; }
}
