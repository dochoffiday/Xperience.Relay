using System.Text.RegularExpressions;

namespace Xperience.Relay.Core.Tests;

public class RelayApiKeyGeneratorTests
{
    [Fact]
    public void Generate_ProducesAPrefixedUrlSafeKey()
    {
        var generated = RelayApiKeyGenerator.Generate();

        Assert.Matches(new Regex("^rly_[A-Za-z0-9_-]{43}$"), generated.Key);
    }

    [Fact]
    public void Generate_ReturnsADifferentKeyEachTime()
    {
        var keys = Enumerable.Range(0, 200).Select(_ => RelayApiKeyGenerator.Generate().Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void Generate_PrefixIsTheStartOfTheKey_AndHashIsTheKeysHash()
    {
        var generated = RelayApiKeyGenerator.Generate();

        Assert.StartsWith(generated.Prefix, generated.Key);
        Assert.Equal(10, generated.Prefix.Length);
        Assert.Equal(RelayApiKeyGenerator.Hash(generated.Key), generated.Hash);
    }

    [Fact]
    public void Hash_IsUppercaseHexSha256()
    {
        Assert.Equal(
            "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD",
            RelayApiKeyGenerator.Hash("abc"));
    }

    [Fact]
    public void Hash_DiffersForDifferentKeys()
    {
        Assert.NotEqual(RelayApiKeyGenerator.Hash("rly_a"), RelayApiKeyGenerator.Hash("rly_b"));
    }
}
