using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class ProtectedObjectsTests
{
    [Theory]
    [InlineData("relay.apikey")]
    [InlineData("RELAY.APIKEY")]
    [InlineData("Relay.ApiKey")]
    public void TheApiKeyObjectType_IsProtected_RegardlessOfCase(string objectType)
    {
        Assert.True(ProtectedObjects.IsProtectedObjectType(objectType));
    }

    [Theory]
    [InlineData("cms.user")]
    [InlineData("custom.redirect")]
    [InlineData("")]
    public void OtherObjectTypes_AreNotProtected(string objectType)
    {
        Assert.False(ProtectedObjects.IsProtectedObjectType(objectType));
    }
}
