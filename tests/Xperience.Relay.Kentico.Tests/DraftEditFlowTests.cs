using CMS.ContentEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class DraftEditFlowTests
{
    private const string Subject = "web page 5 (en)";
    private const string Success = "Updated 1 field(s) on web page 5.";

    private sealed class Steps
    {
        public List<string> Log { get; } = [];

        public bool CreateOk { get; set; } = true;
        public string? SaveProblem { get; set; }
        public bool PublishOk { get; set; } = true;
        public bool DiscardOk { get; set; } = true;

        public Exception? CreateThrows { get; set; }
        public Exception? SaveThrows { get; set; }
        public Exception? PublishThrows { get; set; }
        public Exception? DiscardThrows { get; set; }
        public Exception? ForgetThrows { get; set; }

        public DraftOperations Operations => new(
            CreateDraft: () => Run("draft", CreateThrows, CreateOk),
            SaveDraft: () => Run("save", SaveThrows, SaveProblem),
            Publish: () => Run("publish", PublishThrows, PublishOk),
            DiscardDraft: () => Run("discard", DiscardThrows, DiscardOk),
            ForgetCachedCopies: () =>
            {
                Log.Add("forget");

                if (ForgetThrows is not null)
                {
                    throw ForgetThrows;
                }
            });

        public Task<RelayCommandResult> Edit(ContentVersionState? state) =>
            DraftEditFlow.EditAsync(Subject, state, Operations, Success);

        private Task<T> Run<T>(string name, Exception? throws, T result)
        {
            Log.Add(name);

            return throws is null ? Task.FromResult(result) : throw throws;
        }
    }

    private static ContentVersionState State(VersionStatus latest, bool hasPublished = false) => new(latest, hasPublished);

    private static readonly ContentVersionState Live = State(VersionStatus.Published, hasPublished: true);

    [Fact]
    public async Task WhenTheItemIsNotFoundInThatLanguage_Fails()
    {
        var steps = new Steps();

        var result = await steps.Edit(null);

        Assert.False(result.Success);
        Assert.Equal("Web page 5 (en) was not found in that language.", result.Error);
        Assert.Empty(steps.Log);
    }

    [Fact]
    public async Task APublishedItem_GetsADraft_IsEdited_AndIsPublishedAgain()
    {
        var steps = new Steps();

        var result = await steps.Edit(Live);

        Assert.True(result.Success);
        Assert.Equal(Success, result.Message);
        Assert.Equal(["draft", "save", "publish"], steps.Log);
    }

    [Fact]
    public async Task AnUnpublishedItem_GetsADraftAndStaysADraft_AndSaysSo()
    {
        var steps = new Steps();

        var result = await steps.Edit(State(VersionStatus.Unpublished));

        Assert.True(result.Success);
        Assert.Equal($"{Success} Saved as a draft; it is not published.", result.Message);
        Assert.Equal(["draft", "save"], steps.Log);
    }

    [Theory]
    [InlineData(VersionStatus.InitialDraft, false, "Saved as a draft; it is not published.")]
    [InlineData(VersionStatus.Draft, true, "Saved as a draft; the published version is unchanged.")]
    public async Task AnItemThatAlreadyHasADraft_IsEditedInPlace_NotPublished_AndSaysWhatIsLive(VersionStatus latest, bool hasPublished, string note)
    {
        var steps = new Steps();

        var result = await steps.Edit(State(latest, hasPublished));

        Assert.True(result.Success);
        Assert.Equal($"{Success} {note}", result.Message);
        Assert.Equal(["save"], steps.Log);
    }

    [Fact]
    public async Task WhenKenticoWillNotCreateTheDraft_FailsWithoutEditing()
    {
        var steps = new Steps { CreateOk = false };

        var result = await steps.Edit(Live);

        Assert.False(result.Success);
        Assert.Contains("create a draft", result.Error);
        Assert.Equal(["draft"], steps.Log);
    }

    [Fact]
    public async Task WhenTheChangeCannotBeSaved_FailsWithItsReason_DoesNotPublish_AndTakesTheNewDraftBack()
    {
        var steps = new Steps { SaveProblem = "nope" };

        var result = await steps.Edit(Live);

        Assert.False(result.Success);
        Assert.Equal("nope", result.Error);
        Assert.Equal(["draft", "save", "discard"], steps.Log);
    }

    [Fact]
    public async Task WhenTheNewDraftCannotBeTakenBack_TheFailureSaysSo()
    {
        var steps = new Steps { SaveProblem = "nope", DiscardOk = false };

        var result = await steps.Edit(Live);

        Assert.False(result.Success);
        Assert.Equal("nope The draft made for the edit could not be removed.", result.Error);
    }

    [Theory]
    [InlineData(VersionStatus.Draft, true)]
    [InlineData(VersionStatus.InitialDraft, false)]
    [InlineData(VersionStatus.Unpublished, false)]
    public async Task WhenTheItemWasNotPublished_AFailedSaveDoesNotDiscardAnything(VersionStatus latest, bool hasPublished)
    {
        // An existing draft is somebody's work, and Kentico can't take back the draft it makes of an unpublished item.
        var steps = new Steps { SaveProblem = "nope" };

        var result = await steps.Edit(State(latest, hasPublished));

        Assert.False(result.Success);
        Assert.Equal("nope", result.Error);
        Assert.DoesNotContain("discard", steps.Log);
    }

    [Fact]
    public async Task WhenKenticoWillNotPublishTheEdit_FailsAndSaysTheLiveVersionIsUnchanged()
    {
        var steps = new Steps { PublishOk = false };

        var result = await steps.Edit(Live);

        Assert.False(result.Success);
        Assert.Contains("saved as a draft", result.Error);
        Assert.Contains("live version is unchanged", result.Error);
        Assert.DoesNotContain("discard", steps.Log);
    }

    [Fact]
    public async Task WhenSavingThrows_CachedCopiesAreDropped_TheNewDraftIsTakenBack_AndTheExceptionSurfaces()
    {
        var steps = new Steps { SaveThrows = new FormatException("bad value") };

        var error = await Assert.ThrowsAsync<FormatException>(() => steps.Edit(Live));

        Assert.Equal("bad value", error.Message);
        Assert.Equal(["draft", "save", "forget", "discard"], steps.Log);
    }

    [Theory]
    [InlineData(VersionStatus.Draft, true)]
    [InlineData(VersionStatus.InitialDraft, false)]
    [InlineData(VersionStatus.Unpublished, false)]
    public async Task WhenSavingThrowsOnAnItemThatWasNotPublished_CachedCopiesAreDropped_AndNothingIsDiscarded(VersionStatus latest, bool hasPublished)
    {
        var steps = new Steps { SaveThrows = new FormatException("bad value") };

        await Assert.ThrowsAsync<FormatException>(() => steps.Edit(State(latest, hasPublished)));

        Assert.Contains("forget", steps.Log);
        Assert.DoesNotContain("discard", steps.Log);
    }

    [Fact]
    public async Task WhenCreatingTheDraftThrows_CachedCopiesAreDropped_AndTheExceptionSurfaces()
    {
        var steps = new Steps { CreateThrows = new TimeoutException("slow") };

        await Assert.ThrowsAsync<TimeoutException>(() => steps.Edit(Live));

        Assert.Equal(["draft", "forget"], steps.Log);
    }

    [Fact]
    public async Task WhenPublishingThrows_CachedCopiesAreDropped_AndTheDraftWithTheEditIsKept()
    {
        var steps = new Steps { PublishThrows = new TimeoutException("slow") };

        await Assert.ThrowsAsync<TimeoutException>(() => steps.Edit(Live));

        Assert.Equal(["draft", "save", "publish", "forget"], steps.Log);
    }

    [Fact]
    public async Task WhenTheCleanUpItselfFails_TheOriginalExceptionStillSurfaces()
    {
        var steps = new Steps
        {
            SaveThrows = new FormatException("bad value"),
            DiscardThrows = new InvalidOperationException("nope"),
            ForgetThrows = new InvalidOperationException("also nope"),
        };

        var error = await Assert.ThrowsAsync<FormatException>(() => steps.Edit(Live));

        Assert.Equal("bad value", error.Message);
    }

    [Fact]
    public async Task WhenTheDraftCannotBeTakenBackBecauseDiscardingThrows_TheFailureSaysSo()
    {
        var steps = new Steps { SaveProblem = "nope", DiscardThrows = new InvalidOperationException("nope") };

        var result = await steps.Edit(Live);

        Assert.False(result.Success);
        Assert.Equal("nope The draft made for the edit could not be removed.", result.Error);
        Assert.Contains("forget", steps.Log);
    }

    [Fact]
    public void SavedOrError_IsNullOnSuccessAndExplainsOtherwise()
    {
        Assert.Null(DraftEditFlow.SavedOrError(true, Subject));
        Assert.Equal("Kentico did not save the changes to web page 5 (en). Check the event log.", DraftEditFlow.SavedOrError(false, Subject));
    }
}
