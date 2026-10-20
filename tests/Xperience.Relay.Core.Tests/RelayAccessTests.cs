using Xperience.Relay.Contracts;

namespace Xperience.Relay.Core.Tests;

public class RelayAccessTests
{
    // Adding a command means choosing its tier here too. Unclassified commands default to Admin.
    private static readonly Dictionary<string, RelayAccess> ExpectedTiers = new()
    {
        ["get-page-info"] = RelayAccess.Read,
        ["get-page"] = RelayAccess.Read,
        ["get-content-info"] = RelayAccess.Read,
        ["get-content"] = RelayAccess.Read,
        ["query-web-page-items"] = RelayAccess.Read,
        ["query-reusable-items"] = RelayAccess.Read,
        ["search-content"] = RelayAccess.Read,
        ["get-taxonomies"] = RelayAccess.Read,
        ["get-tags"] = RelayAccess.Read,
        ["get-content-item-usage"] = RelayAccess.Read,
        ["get-form-submissions"] = RelayAccess.Read,

        ["move-web-page"] = RelayAccess.Write,
        ["move-content-item"] = RelayAccess.Write,
        ["get-content-hub-folder"] = RelayAccess.Write,
        ["create-content-item"] = RelayAccess.Write,
        ["create-web-page"] = RelayAccess.Write,
        ["update-web-page"] = RelayAccess.Write,
        ["update-content-item"] = RelayAccess.Write,
        ["update-slug"] = RelayAccess.Write,
        ["publish-web-page"] = RelayAccess.Write,
        ["unpublish-web-page"] = RelayAccess.Write,
        ["publish-content-item"] = RelayAccess.Write,
        ["unpublish-content-item"] = RelayAccess.Write,
        ["delete-web-page"] = RelayAccess.Write,
        ["delete-content-item"] = RelayAccess.Write,
        ["reoptimize-asset"] = RelayAccess.Write,
        ["rename-asset"] = RelayAccess.Write,
        ["create-taxonomy"] = RelayAccess.Write,
        ["update-taxonomy"] = RelayAccess.Write,
        ["delete-taxonomy"] = RelayAccess.Write,
        ["create-tag"] = RelayAccess.Write,
        ["update-tag"] = RelayAccess.Write,
        ["move-tag"] = RelayAccess.Write,
        ["delete-tag"] = RelayAccess.Write,

        ["query-custom-objects"] = RelayAccess.Admin,
        ["create-custom-object"] = RelayAccess.Admin,
        ["update-custom-object"] = RelayAccess.Admin,
        ["delete-custom-object"] = RelayAccess.Admin,
        ["delete-contacts"] = RelayAccess.Admin,
        ["query-sql"] = RelayAccess.Admin,
        ["execute-sql"] = RelayAccess.Admin,
    };

    public static TheoryData<string, RelayAccess> Tiers()
    {
        var data = new TheoryData<string, RelayAccess>();
        foreach (var (verb, access) in ExpectedTiers)
        {
            data.Add(verb, access);
        }

        return data;
    }

    private static RelayVerbRegistry ContractsRegistry() => new([typeof(RelayAccess).Assembly]);

    [Theory]
    [MemberData(nameof(Tiers))]
    public void Verb_RequiresItsClassifiedTier(string verb, RelayAccess expected)
    {
        Assert.True(ContractsRegistry().TryGetRequiredAccess(verb, out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void EveryRegisteredVerb_IsInTheExpectedTierTable()
    {
        var registered = ContractsRegistry().Verbs.OrderBy(v => v, StringComparer.Ordinal);
        var expected = ExpectedTiers.Keys.OrderBy(v => v, StringComparer.Ordinal);

        Assert.Equal(expected, registered);
    }

    [Fact]
    public void CommandWithoutAnExplicitTier_DefaultsToAdmin()
    {
        var registry = new RelayVerbRegistry([typeof(RelayAccessTests).Assembly]);

        Assert.True(registry.TryGetRequiredAccess("test-unclassified", out var access));
        Assert.Equal(RelayAccess.Admin, access);
    }

    [Fact]
    public void UnknownVerb_HasNoRequiredTier()
    {
        Assert.False(ContractsRegistry().TryGetRequiredAccess("no-such-verb", out _));
    }

    [Fact]
    public void Tiers_AreOrderedSoHigherTiersIncludeLowerOnes()
    {
        Assert.True(RelayAccess.Read < RelayAccess.Write);
        Assert.True(RelayAccess.Write < RelayAccess.Admin);
    }

    [RelayCommand("test-unclassified")]
    private sealed class UnclassifiedCommand : IRelayCommand;
}
