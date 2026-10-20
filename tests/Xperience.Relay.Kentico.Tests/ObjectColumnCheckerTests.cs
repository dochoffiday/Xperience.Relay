using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class ObjectColumnCheckerTests
{
    private static readonly string[] Known = ["CountryID", "CountryName", "CountryDisplayName"];

    [Fact]
    public void ColumnsTheTypeHas_Pass_WhateverTheirCase()
    {
        Assert.Null(ObjectColumnChecker.Check("cms.country", Known, ["countryname", "CountryDisplayName"]));
    }

    [Fact]
    public void AColumnTheTypeLacks_IsNamedWithTheType()
    {
        var problem = ObjectColumnChecker.Check("cms.country", Known, ["CountryName", "CountryFlag"]);

        Assert.Equal("Column 'CountryFlag' was not found on object type 'cms.country'.", problem);
    }

    [Fact]
    public void SeveralMissingColumns_AreNamedTogether_OnceEach()
    {
        var problem = ObjectColumnChecker.Check("cms.country", Known, ["Flag", "flag", "Anthem"]);

        Assert.Equal("Columns 'Flag', 'Anthem' were not found on object type 'cms.country'.", problem);
    }

    [Fact]
    public void ATypeThatDoesNotListItsColumns_IsNotChecked()
    {
        Assert.Null(ObjectColumnChecker.Check("custom.thing", [], ["Anything"]));
    }
}
