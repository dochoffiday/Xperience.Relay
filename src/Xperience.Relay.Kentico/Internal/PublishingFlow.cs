using CMS.ContentEngine;
using Xperience.Relay.Contracts;

namespace Xperience.Relay.Kentico.Internal;

public enum PublishPlan
{
    NotFound,
    AlreadyPublished,
    PublishDraft,
    CreateDraftThenPublish,
}

public enum UnpublishPlan
{
    NotFound,
    Unpublish,
    AlreadyUnpublished,
    DraftOnly,
    DraftBlocksUnpublish,
}

/// <summary>
/// What publishing or unpublishing has to do from each version state. Kentico only accepts publish on a draft and
/// unpublish on a published version, and answers anything else with <c>false</c> -- so asking first is the only way
/// to tell "nothing to do" from "failed".
/// </summary>
internal static class PublishingPlan
{
    public static PublishPlan ForPublish(ContentVersionState? state) => state switch
    {
        null => PublishPlan.NotFound,
        { Latest: VersionStatus.Published } => PublishPlan.AlreadyPublished,
        { Latest: VersionStatus.Unpublished } => PublishPlan.CreateDraftThenPublish,
        _ => PublishPlan.PublishDraft,
    };

    public static UnpublishPlan ForUnpublish(ContentVersionState? state) => state switch
    {
        null => UnpublishPlan.NotFound,
        { Latest: VersionStatus.Published } => UnpublishPlan.Unpublish,
        { Latest: VersionStatus.Unpublished } => UnpublishPlan.AlreadyUnpublished,
        { HasPublishedVersion: true } => UnpublishPlan.DraftBlocksUnpublish,
        _ => UnpublishPlan.DraftOnly,
    };
}

/// <summary>
/// Runs <see cref="PublishingPlan"/> against Kentico's managers and reports what actually happened. Asking for a state
/// the item is already in succeeds without doing anything, so both verbs are safe to repeat.
/// </summary>
internal static class PublishingFlow
{
    /// <param name="subject">What the message talks about, e.g. <c>web page 5 (en)</c>.</param>
    /// <param name="state">The item's current state in that language, or null if it has none.</param>
    /// <param name="createDraft">Kentico's TryCreateDraft for the item.</param>
    /// <param name="publish">Kentico's TryPublish for the item.</param>
    public static async Task<RelayCommandResult> PublishAsync(
        string subject,
        ContentVersionState? state,
        Func<Task<bool>> createDraft,
        Func<Task<bool>> publish)
    {
        var plan = PublishingPlan.ForPublish(state);

        if (plan == PublishPlan.NotFound)
        {
            return RelayCommandResult.Fail($"{Sentence(subject)} was not found in that language.");
        }

        if (plan == PublishPlan.AlreadyPublished)
        {
            return RelayCommandResult.Ok($"{Sentence(subject)} is already published.");
        }

        // An unpublished item has no draft to publish, so make one first.
        if (plan == PublishPlan.CreateDraftThenPublish && !await createDraft())
        {
            return RelayCommandResult.Fail($"Kentico would not create the draft needed to publish {subject} again. Check the event log.");
        }

        return await publish()
            ? RelayCommandResult.Ok($"Published {subject}.")
            : RelayCommandResult.Fail($"Kentico did not publish {subject}. Check the event log.");
    }

    /// <param name="subject">What the message talks about, e.g. <c>web page 5 (en)</c>.</param>
    /// <param name="state">The item's current state in that language, or null if it has none.</param>
    /// <param name="unpublish">Kentico's TryUnpublish for the item.</param>
    public static async Task<RelayCommandResult> UnpublishAsync(
        string subject,
        ContentVersionState? state,
        Func<Task<bool>> unpublish)
    {
        switch (PublishingPlan.ForUnpublish(state))
        {
            case UnpublishPlan.NotFound:
                return RelayCommandResult.Fail($"{Sentence(subject)} was not found in that language.");

            case UnpublishPlan.AlreadyUnpublished:
                return RelayCommandResult.Ok($"{Sentence(subject)} is already unpublished.");

            case UnpublishPlan.DraftOnly:
                return RelayCommandResult.Ok($"{Sentence(subject)} is not published; it only has a draft.");

            case UnpublishPlan.DraftBlocksUnpublish:
                return RelayCommandResult.Fail(
                    $"{Sentence(subject)} is published and also has a draft with unpublished changes, and Kentico won't unpublish it while that draft exists. Publish or discard the draft first.");

            default:
                return await unpublish()
                    ? RelayCommandResult.Ok($"Unpublished {subject}.")
                    : RelayCommandResult.Fail($"Kentico did not unpublish {subject}. Check the event log.");
        }
    }

    private static string Sentence(string text) => char.ToUpperInvariant(text[0]) + text[1..];
}
