using System.Text.Json;
using Xperience.Relay.Contracts.Commands;

namespace Xperience.Relay.Core.Tests;

// External callers (such as the CancerCare kit) send these flags as JSON, so the names are part of the contract.
public class QueryCommandWireFormatTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [Fact]
    public void QueryFlags_DefaultToOff()
    {
        var command = JsonSerializer.Deserialize<QueryWebPageItemsCommand>("{}", Web)!;

        Assert.False(command.PublishedOnly);
        Assert.False(command.IncludeUrls);
        Assert.False(command.IncludeStatus);
    }

    [Fact]
    public void QueryWebPageItems_ReadsTheFlagsFromCamelCaseJson()
    {
        var command = JsonSerializer.Deserialize<QueryWebPageItemsCommand>(
            """{"publishedOnly":true,"includeUrls":true,"includeStatus":true}""", Web)!;

        Assert.True(command.PublishedOnly);
        Assert.True(command.IncludeUrls);
        Assert.True(command.IncludeStatus);
    }

    [Fact]
    public void QueryReusableItems_ReadsPublishedOnly()
    {
        var command = JsonSerializer.Deserialize<QueryReusableItemsCommand>("""{"publishedOnly":true}""", Web)!;

        Assert.True(command.PublishedOnly);
    }
}
