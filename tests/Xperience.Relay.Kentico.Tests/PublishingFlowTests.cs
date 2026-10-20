using CMS.ContentEngine;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class PublishingPlanTests
{
    public static TheoryData<VersionStatus?, bool, PublishPlan> PublishCases => new()
    {
        { null, false, PublishPlan.NotFound },
        { VersionStatus.Published, true, PublishPlan.AlreadyPublished },
        { VersionStatus.Draft, true, PublishPlan.PublishDraft },
        { VersionStatus.Draft, false, PublishPlan.PublishDraft },
        { VersionStatus.InitialDraft, false, PublishPlan.PublishDraft },
        { VersionStatus.Unpublished, false, PublishPlan.CreateDraftThenPublish },
    };

    public static TheoryData<VersionStatus?, bool, UnpublishPlan> UnpublishCases => new()
    {
        { null, false, UnpublishPlan.NotFound },
        { VersionStatus.Published, true, UnpublishPlan.Unpublish },
        { VersionStatus.Unpublished, false, UnpublishPlan.AlreadyUnpublished },
        { VersionStatus.InitialDraft, false, UnpublishPlan.DraftOnly },
        { VersionStatus.Draft, true, UnpublishPlan.DraftBlocksUnpublish },
        { VersionStatus.Draft, false, UnpublishPlan.DraftOnly },
    };

    private static ContentVersionState? State(VersionStatus? latest, bool hasPublished) =>
        latest is null ? null : new ContentVersionState(latest.Value, hasPublished);

    [Theory]
    [MemberData(nameof(PublishCases))]
    public void Publish_PlansFromTheLatestVersion(VersionStatus? latest, bool hasPublished, PublishPlan expected)
    {
        Assert.Equal(expected, PublishingPlan.ForPublish(State(latest, hasPublished)));
    }

    [Theory]
    [MemberData(nameof(UnpublishCases))]
    public void Unpublish_PlansFromTheLatestVersion(VersionStatus? latest, bool hasPublished, UnpublishPlan expected)
    {
        Assert.Equal(expected, PublishingPlan.ForUnpublish(State(latest, hasPublished)));
    }
}

public class PublishingFlowTests
{
    private const string Subject = "web page 5 (en)";

    private sealed class Calls
    {
        public List<string> Log { get; } = [];

        public Func<Task<bool>> Step(string name, bool result) => () =>
        {
            Log.Add(name);
            return Task.FromResult(result);
        };
    }

    private static ContentVersionState State(VersionStatus latest, bool hasPublished = false) => new(latest, hasPublished);

    // ---- publish

    [Fact]
    public async Task Publish_WhenThePageIsNotFound_Fails()
    {
        var calls = new Calls();

        var result = await PublishingFlow.PublishAsync(Subject, null, calls.Step("draft", true), calls.Step("publish", true));

        Assert.False(result.Success);
        Assert.Equal("Web page 5 (en) was not found in that language.", result.Error);
        Assert.Empty(calls.Log);
    }

    [Fact]
    public async Task Publish_WhenAlreadyPublished_SucceedsWithoutTouchingKentico()
    {
        var calls = new Calls();

        var result = await PublishingFlow.PublishAsync(Subject, State(VersionStatus.Published), calls.Step("draft", true), calls.Step("publish", true));

        Assert.True(result.Success);
        Assert.Equal("Web page 5 (en) is already published.", result.Message);
        Assert.Empty(calls.Log);
    }

    [Theory]
    [InlineData(VersionStatus.Draft)]
    [InlineData(VersionStatus.InitialDraft)]
    public async Task Publish_OfADraft_JustPublishes(VersionStatus latest)
    {
        var calls = new Calls();

        var result = await PublishingFlow.PublishAsync(Subject, State(latest), calls.Step("draft", true), calls.Step("publish", true));

        Assert.True(result.Success);
        Assert.Equal("Published web page 5 (en).", result.Message);
        Assert.Equal(["publish"], calls.Log);
    }

    [Fact]
    public async Task Publish_OfAnUnpublishedPage_CreatesADraftFirst()
    {
        var calls = new Calls();

        var result = await PublishingFlow.PublishAsync(Subject, State(VersionStatus.Unpublished), calls.Step("draft", true), calls.Step("publish", true));

        Assert.True(result.Success);
        Assert.Equal(["draft", "publish"], calls.Log);
    }

    [Fact]
    public async Task Publish_WhenKenticoDeclinesToCreateTheDraft_FailsAndDoesNotPublish()
    {
        var calls = new Calls();

        var result = await PublishingFlow.PublishAsync(Subject, State(VersionStatus.Unpublished), calls.Step("draft", false), calls.Step("publish", true));

        Assert.False(result.Success);
        Assert.Contains("draft", result.Error);
        Assert.Equal(["draft"], calls.Log);
    }

    [Fact]
    public async Task Publish_WhenKenticoDeclinesToPublish_FailsInsteadOfClaimingSuccess()
    {
        var calls = new Calls();

        var result = await PublishingFlow.PublishAsync(Subject, State(VersionStatus.Draft), calls.Step("draft", true), calls.Step("publish", false));

        Assert.False(result.Success);
        Assert.Equal("Kentico did not publish web page 5 (en). Check the event log.", result.Error);
    }

    // ---- unpublish

    [Fact]
    public async Task Unpublish_WhenThePageIsNotFound_Fails()
    {
        var calls = new Calls();

        var result = await PublishingFlow.UnpublishAsync(Subject, null, calls.Step("unpublish", true));

        Assert.False(result.Success);
        Assert.Empty(calls.Log);
    }

    [Fact]
    public async Task Unpublish_OfAPublishedPage_Unpublishes()
    {
        var calls = new Calls();

        var result = await PublishingFlow.UnpublishAsync(Subject, State(VersionStatus.Published), calls.Step("unpublish", true));

        Assert.True(result.Success);
        Assert.Equal("Unpublished web page 5 (en).", result.Message);
        Assert.Equal(["unpublish"], calls.Log);
    }

    [Fact]
    public async Task Unpublish_WhenKenticoDeclines_FailsInsteadOfClaimingSuccess()
    {
        var calls = new Calls();

        var result = await PublishingFlow.UnpublishAsync(Subject, State(VersionStatus.Published), calls.Step("unpublish", false));

        Assert.False(result.Success);
        Assert.Equal("Kentico did not unpublish web page 5 (en). Check the event log.", result.Error);
    }

    [Theory]
    [InlineData(VersionStatus.Unpublished, false, "is already unpublished")]
    [InlineData(VersionStatus.InitialDraft, false, "is not published; it only has a draft")]
    [InlineData(VersionStatus.Draft, false, "is not published; it only has a draft")]
    public async Task Unpublish_WhenNothingIsLive_SucceedsWithoutTouchingKentico(VersionStatus latest, bool hasPublished, string expectedText)
    {
        var calls = new Calls();

        var result = await PublishingFlow.UnpublishAsync(Subject, State(latest, hasPublished), calls.Step("unpublish", true));

        Assert.True(result.Success);
        Assert.Contains(expectedText, result.Message);
        Assert.Empty(calls.Log);
    }

    [Fact]
    public async Task Unpublish_OfAPublishedPageWithAPendingDraft_FailsWithAnExplanation()
    {
        var calls = new Calls();

        var result = await PublishingFlow.UnpublishAsync(Subject, State(VersionStatus.Draft, hasPublished: true), calls.Step("unpublish", true));

        Assert.False(result.Success);
        Assert.Contains("draft", result.Error);
        Assert.Contains("publish or discard", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(calls.Log);
    }
}
