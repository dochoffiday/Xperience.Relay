using System.Text.Json;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class QueryInputCheckerTests
{
    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Theory]
    [InlineData("CafeName")]
    [InlineData("_internal")]
    [InlineData("A1")]
    [InlineData("ContentItemCommonDataVersionStatus")]
    public void PlainNames_AreNames(string name)
    {
        Assert.True(QueryInputChecker.IsName(name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1Name")]
    [InlineData("Cafe Name")]
    [InlineData("Cafe.Name")]
    [InlineData("[CafeName]")]
    [InlineData("CafeName;")]
    [InlineData("Cafe--Name")]
    [InlineData("Cafe,Name")]
    [InlineData("(CafeName)")]
    [InlineData("*")]
    [InlineData("Café")]
    [InlineData("CafeName\n")]
    public void AnythingElse_IsNotAName(string? name)
    {
        Assert.False(QueryInputChecker.IsName(name));
    }

    [Fact]
    public void ANameCanBeAsLongAsSqlServerAllows_ButNotLonger()
    {
        Assert.True(QueryInputChecker.IsName(new string('a', QueryInputChecker.MaxNameLength)));
        Assert.False(QueryInputChecker.IsName(new string('a', QueryInputChecker.MaxNameLength + 1)));
    }

    [Theory]
    [InlineData("DancingGoat.ArticlePage", true)]
    [InlineData("cms.user", true)]
    [InlineData("BizForm.Form_2023_09_12", true)]
    [InlineData("Plain", true)]
    [InlineData("", false)]
    [InlineData(".Name", false)]
    [InlineData("Name.", false)]
    [InlineData("A..B", false)]
    [InlineData("A.B C", false)]
    [InlineData("A.B;", false)]
    public void CodeNames_AreNamesSeparatedByDots(string name, bool expected)
    {
        Assert.Equal(expected, QueryInputChecker.IsCodeName(name));
    }

    [Fact]
    public void AQueryThatIsFine_HasNoProblem()
    {
        var problem = QueryInputChecker.CheckQuery(
            ["DancingGoat.Cafe"],
            ["ContentItemID", "CafeName"],
            new Dictionary<string, JsonElement> { ["CafeName"] = Json("\"Seattle\"") });

        Assert.Null(problem);
    }

    [Fact]
    public void AColumnThatIsNotAName_IsRefusedAndShown()
    {
        var problem = QueryInputChecker.CheckQuery(["DancingGoat.Cafe"], ["CafeName", "Cafe Name"], null);

        Assert.Equal("Column name 'Cafe Name' isn't valid: use letters, digits and underscores only.", problem);
    }

    [Fact]
    public void SeveralBadColumns_AreReportedTogether_OnceEach()
    {
        var problem = QueryInputChecker.CheckQuery(["DancingGoat.Cafe"], ["a b", "c;", "a b", "Fine"], null);

        Assert.Equal("Column names 'a b', 'c;' aren't valid: use letters, digits and underscores only.", problem);
    }

    [Fact]
    public void ABadWhereEqualsColumn_IsRefused_LikeAColumn()
    {
        var problem = QueryInputChecker.CheckQuery(
            ["DancingGoat.Cafe"],
            ["CafeName"],
            new Dictionary<string, JsonElement> { ["Cafe Name"] = Json("1") });

        Assert.Equal("Column name 'Cafe Name' isn't valid: use letters, digits and underscores only.", problem);
    }

    [Fact]
    public void AnEmptyOrMissingColumn_IsShownAsEmpty()
    {
        var problem = QueryInputChecker.CheckQuery(["DancingGoat.Cafe"], [null, ""], null);

        Assert.Equal("Column names '' aren't valid: use letters, digits and underscores only.", problem);
    }

    [Fact]
    public void AMessageShowsFewNames_EachCutShort_WithNothingUnprintable()
    {
        var columns = Enumerable.Range(1, 8).Select(i => $"bad name {i}").ToList();
        columns.Add(new string('x', 60) + " y");
        columns.Add("bell\u0007");

        var problem = QueryInputChecker.CheckQuery(["DancingGoat.Cafe"], columns, null)!;

        Assert.Contains("'bad name 1'", problem);
        Assert.Contains("and 5 more", problem);
        Assert.DoesNotContain("bad name 6", problem);
        Assert.DoesNotContain(new string('x', 60), problem);
        Assert.DoesNotContain('\u0007', problem);
    }

    [Fact]
    public void ABadContentTypeName_IsRefused()
    {
        var problem = QueryInputChecker.CheckQuery(["DancingGoat.Cafe", "Dancing Goat"], ["CafeName"], null);

        Assert.Equal("'Dancing Goat' is not a content type name; use the code name, such as DancingGoat.ArticlePage.", problem);
    }

    [Fact]
    public void TooManyContentTypesColumnsAndFilters_AreRefused()
    {
        var types = Enumerable.Range(1, QueryInputChecker.MaxContentTypes + 1).Select(i => $"Ns.Type{i}").ToList();
        var columns = Enumerable.Range(1, QueryInputChecker.MaxColumns + 1).Select(i => $"Column{i}").ToList();
        var filters = Enumerable.Range(1, QueryInputChecker.MaxFilters + 1).ToDictionary(i => $"Column{i}", _ => Json("1"));

        var problem = QueryInputChecker.CheckQuery(types, columns, filters);

        Assert.Equal(
            "ContentTypeNames can list at most 25 content types. Columns can list at most 500 columns. WhereEquals can have at most 50 entries.",
            problem);
    }

    [Fact]
    public void AListOverItsLimit_IsRefusedWithoutLookingAtWhatIsInIt()
    {
        var columns = Enumerable.Range(1, QueryInputChecker.MaxColumns + 1).Select(i => $"bad name {i}").ToList();

        var problem = QueryInputChecker.CheckQuery(["DancingGoat.Cafe"], columns, null);

        Assert.Equal("Columns can list at most 500 columns.", problem);
    }

    [Theory]
    [InlineData("\"text\"")]
    [InlineData("12.5")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    public void PlainValues_AreFine_InAFilter(string json)
    {
        Assert.Null(QueryInputChecker.CheckWhereEquals(new Dictionary<string, JsonElement> { ["CafeCity"] = Json(json) }));
    }

    [Theory]
    [InlineData("[1, 2]")]
    [InlineData("{\"a\": 1}")]
    public void AValueThatIsNotPlain_IsRefused_NotQuietlyReadAsNull(string json)
    {
        var problem = QueryInputChecker.CheckWhereEquals(new Dictionary<string, JsonElement> { ["CafeCity"] = Json(json) });

        Assert.Equal("The WhereEquals value for 'CafeCity' must be text, a number, true, false or null.", problem);
    }

    [Fact]
    public void NoFilter_IsFine()
    {
        Assert.Null(QueryInputChecker.CheckWhereEquals(null));
        Assert.Null(QueryInputChecker.CheckWhereEquals(new Dictionary<string, JsonElement>()));
    }

    [Fact]
    public void SearchText_HasALimit()
    {
        Assert.Null(QueryInputChecker.CheckSearchText(new string('a', QueryInputChecker.MaxSearchTextLength)));
        Assert.Equal("Filter can be at most 1000 characters.", QueryInputChecker.CheckSearchText(new string('a', QueryInputChecker.MaxSearchTextLength + 1)));
    }
}
