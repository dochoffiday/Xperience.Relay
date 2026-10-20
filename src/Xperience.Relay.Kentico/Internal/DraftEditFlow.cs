using CMS.ContentEngine;
using Xperience.Relay.Contracts;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>The Kentico operations an edit is made of, for one item in one language.</summary>
/// <param name="CreateDraft">Kentico's TryCreateDraft for the item.</param>
/// <param name="SaveDraft">Writes the change into the draft; returns null when it was saved, otherwise why not.</param>
/// <param name="Publish">Kentico's TryPublish for the item.</param>
/// <param name="DiscardDraft">Kentico's TryDiscardDraft for the item.</param>
/// <param name="ForgetCachedCopies">Drops Kentico's cached copies of the item's latest version (see <see cref="FailedWriteCacheCleaner"/>).</param>
internal sealed record DraftOperations(
    Func<Task<bool>> CreateDraft,
    Func<Task<string?>> SaveDraft,
    Func<Task<bool>> Publish,
    Func<Task<bool>> DiscardDraft,
    Action ForgetCachedCopies);

/// <summary>
/// Edits an item through a draft, the way Kentico's own UI does: make a draft (unless one already exists), write the
/// change into it, and publish the result again if the item was live. Every step is checked, so a refusal from Kentico
/// comes back as a failure instead of "Updated". A step that fails leaves nothing behind: the cached copies Kentico
/// changed are dropped, and a draft made for an edit that then fails is taken back out.
/// </summary>
internal static class DraftEditFlow
{
    public static RelayCommandResult NotFound(string subject) =>
        RelayCommandResult.Fail($"{Sentence(subject)} was not found in that language.");

    /// <summary>For the <c>SaveDraft</c> operation: null when Kentico saved the change, otherwise the failure message.</summary>
    public static string? SavedOrError(bool saved, string subject) =>
        saved ? null : $"Kentico did not save the changes to {subject}. Check the event log.";

    /// <param name="subject">What the messages talk about, e.g. <c>web page 5 (en)</c>.</param>
    /// <param name="state">The item's current state in that language, or null if it has none.</param>
    /// <param name="operations">The Kentico operations to run.</param>
    /// <param name="successMessage">What to report when everything worked.</param>
    public static async Task<RelayCommandResult> EditAsync(
        string subject,
        ContentVersionState? state,
        DraftOperations operations,
        string successMessage)
    {
        if (state is not { } current)
        {
            return NotFound(subject);
        }

        // Kentico refuses to make a draft of something that already is one, and only a draft can be edited.
        var needsDraft = current.Latest is VersionStatus.Published or VersionStatus.Unpublished;

        // An item that was live goes live again with the edit. A draft stays a draft, as does an unpublished item.
        var republish = current.Latest == VersionStatus.Published;

        if (needsDraft && !await Guarded(operations, operations.CreateDraft, discardOnFailure: false))
        {
            return RelayCommandResult.Fail($"Kentico would not create a draft of {subject} to edit. Check the event log.");
        }

        // A draft made over a published item can be discarded to put the item back as it was. Over an unpublished item
        // Kentico turns the item into a draft in place, which it can't undo (the item is just as unpublished).
        var canUndoDraft = republish;

        var problem = await Guarded(operations, operations.SaveDraft, discardOnFailure: canUndoDraft);

        if (problem is not null)
        {
            var stuck = canUndoDraft && !await TryDiscardAsync(operations);

            return RelayCommandResult.Fail(stuck ? $"{problem} The draft made for the edit could not be removed." : problem);
        }

        if (republish && !await Guarded(operations, operations.Publish, discardOnFailure: false))
        {
            return RelayCommandResult.Fail(
                $"The changes to {subject} were saved as a draft, but Kentico did not publish them, so the live version is unchanged. Check the event log.");
        }

        return RelayCommandResult.Ok(republish ? successMessage : $"{successMessage} {NotLive(current)}");
    }

    /// <summary>
    /// Runs one Kentico step. If it throws, Kentico's cached copies are dropped (it may have changed them before
    /// failing), a draft made for this edit is discarded if asked, and the exception still surfaces.
    /// </summary>
    private static async Task<T> Guarded<T>(DraftOperations operations, Func<Task<T>> step, bool discardOnFailure)
    {
        try
        {
            return await step();
        }
        catch
        {
            TryForget(operations);

            // A leftover draft stops later edits going live and blocks unpublishing, so don't leave it behind.
            if (discardOnFailure)
            {
                await TryDiscardAsync(operations);
            }

            throw;
        }
    }

    private static string NotLive(ContentVersionState state) => state.HasPublishedVersion
        ? "Saved as a draft; the published version is unchanged."
        : "Saved as a draft; it is not published.";

    private static async Task<bool> TryDiscardAsync(DraftOperations operations)
    {
        try
        {
            return await operations.DiscardDraft();
        }
        catch
        {
            TryForget(operations);

            return false;
        }
    }

    private static void TryForget(DraftOperations operations)
    {
        try
        {
            operations.ForgetCachedCopies();
        }
        catch
        {
            // Best effort: the failure being reported matters more than this cleanup.
        }
    }

    private static string Sentence(string text) => char.ToUpperInvariant(text[0]) + text[1..];
}
