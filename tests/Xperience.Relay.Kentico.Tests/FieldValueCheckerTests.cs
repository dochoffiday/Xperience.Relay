using CMS.DataEngine;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class FieldValueCheckerTests
{
    private const string Type = "Site.Article";

    private static readonly FieldRule[] Rules =
    [
        new("Title", FieldDataType.Text, 10, 0, AllowEmpty: false),
        new("Body", FieldDataType.RichTextHTML, 0, 0, AllowEmpty: true),
        new("Count", FieldDataType.Integer, 0, 0, AllowEmpty: true),
        new("Big", FieldDataType.LongInteger, 0, 0, AllowEmpty: true),
        new("Rating", FieldDataType.Double, 0, 0, AllowEmpty: true),
        new("Price", FieldDataType.Decimal, 19, 4, AllowEmpty: true),
        new("Featured", FieldDataType.Boolean, 0, 0, AllowEmpty: true),
        new("PublishDate", FieldDataType.Date, 0, 0, AllowEmpty: true),
        new("Starts", FieldDataType.DateTime, 0, 0, AllowEmpty: true),
        new("Length", FieldDataType.TimeSpan, 0, 0, AllowEmpty: true),
        new("Key", FieldDataType.Guid, 0, 0, AllowEmpty: true),
        new("Related", FieldDataType.ContentItemReference, 0, 0, AllowEmpty: true),
        new("Labels", FieldDataType.Taxonomy, 0, 0, AllowEmpty: true),
        new("File", FieldDataType.ContentItemAsset, 0, 0, AllowEmpty: true),
    ];

    private static string? Check(params (string Name, object? Value)[] values) =>
        FieldValueChecker.Check(Type, Rules, values.Select(v => KeyValuePair.Create(v.Name, v.Value)), []);

    private static string? CheckTyped(params (string Name, FieldUse Use)[] typed) =>
        FieldValueChecker.Check(Type, Rules, [], typed.Select(t => new TypedField(t.Name, t.Use)));

    [Fact]
    public void ValuesThatFit_Pass()
    {
        Assert.Null(Check(
            ("Title", "Hello"), ("Body", "<p>x</p>"), ("Count", 5), ("Big", 5_000_000_000L), ("Rating", 4.5), ("Price", 9.99m),
            ("Featured", true), ("PublishDate", "2026-10-20"), ("Starts", "2026-10-20T08:30:00Z"), ("Length", "01:30:00"),
            ("Key", "4faf9f31-e84c-4b6c-9015-02f358f1951f")));
    }

    [Fact]
    public void FieldNamesAreMatchedWithoutRegardToCase()
    {
        Assert.Null(Check(("title", "Hello"), ("PUBLISHDATE", "2026-10-20")));
    }

    [Fact]
    public void NullPassesForAFieldThatMayBeEmpty()
    {
        Assert.Null(Check(("PublishDate", null), ("Count", null), ("Related", null)));
    }

    [Fact]
    public void NullIsRefusedForAFieldThatMustHaveAValue()
    {
        Assert.Equal("Field 'Title' can't be null.", Check(("title", null)));
    }

    [Fact]
    public void AnEmptyStringIsStillAStringForATextField()
    {
        Assert.Null(Check(("Title", "")));
    }

    [Theory]
    [InlineData("999999999999999")]
    [InlineData(999999999999999L)]
    [InlineData("-999999999999999.9999")]
    [InlineData("1.123456789")]
    [InlineData(0)]
    public void ADecimalThatFitsTheColumn_Passes(object value)
    {
        Assert.Null(Check(("Price", value)));
    }

    [Theory]
    [InlineData("1000000000000000")]
    [InlineData(1000000000000000L)]
    [InlineData(1e20)]
    [InlineData("-1000000000000000.5")]
    public void ADecimalWithTooManyDigitsBeforeThePoint_IsReported(object value)
    {
        Assert.StartsWith("Field 'Price' allows at most 15 digits before the decimal point, but got ", Check(("Price", value)));
    }

    [Fact]
    public void ADoubleBeyondWhatADecimalCanHold_IsReported()
    {
        Assert.StartsWith("Field 'Price' expects a number, but got ", Check(("Price", double.MaxValue)));
    }

    [Fact]
    public void AFieldTheTypeDoesNotHave_IsReported()
    {
        Assert.Equal("Field 'Nope' was not found on content type 'Site.Article'.", Check(("Title", "Hello"), ("Nope", "x")));
    }

    [Fact]
    public void SeveralUnknownFields_AreReportedTogether()
    {
        Assert.Equal("Fields 'Nope', 'Nada' were not found on content type 'Site.Article'.", Check(("Nope", "x"), ("Title", "Hello"), ("Nada", 1), ("nope", 2)));
    }

    [Fact]
    public void LinkedItemTagAndAssetFields_MustExist()
    {
        Assert.Null(CheckTyped(("Related", FieldUse.LinkedItems), ("labels", FieldUse.Tags), ("FILE", FieldUse.Asset)));
        Assert.Equal("Field 'Missing' was not found on content type 'Site.Article'.", CheckTyped(("Related", FieldUse.LinkedItems), ("Missing", FieldUse.Tags)));
    }

    [Fact]
    public void LinkedItems_AreRefusedByAFieldThatIsNotAContentItemReference()
    {
        Assert.Equal(
            "Field 'Title' is not a content item reference field (it is text), so it can't be set from LinkedItemFields.",
            CheckTyped(("title", FieldUse.LinkedItems)));
    }

    [Fact]
    public void Tags_AreRefusedByAFieldThatIsNotATagField()
    {
        Assert.Equal(
            "Field 'Related' is not a tag field (it is contentitemreference), so it can't be set from TagFields.",
            CheckTyped(("Related", FieldUse.Tags)));
    }

    [Fact]
    public void Assets_AreRefusedByAFieldThatIsNotAnAssetField()
    {
        Assert.Equal(
            "Field 'Body' is not an asset field (it is richtexthtml), so it can't be set from Assets.",
            CheckTyped(("Body", FieldUse.Asset)));
    }

    [Fact]
    public void EachTypedFieldIsJudgedOnItsOwn()
    {
        Assert.Equal(
            "Field 'Title' is not a content item reference field (it is text), so it can't be set from LinkedItemFields. Field 'Related' is not a tag field (it is contentitemreference), so it can't be set from TagFields.",
            CheckTyped(("Title", FieldUse.LinkedItems), ("Labels", FieldUse.Tags), ("Related", FieldUse.Tags)));
    }

    [Fact]
    public void ATypeWithoutKnownFields_RejectsEverything()
    {
        Assert.Equal("Field 'Title' was not found on content type 'Empty'.", FieldValueChecker.Check("Empty", [], [KeyValuePair.Create<string, object?>("Title", "x")], []));
    }

    [Fact]
    public void ATextValueLongerThanTheField_IsReported()
    {
        Assert.Equal("Field 'Title' allows at most 10 characters, but the value has 11.", Check(("Title", "12345678901")));
        Assert.Null(Check(("Title", "1234567890")));
    }

    [Fact]
    public void LongTextHasNoLimit()
    {
        Assert.Null(Check(("Body", new string('x', 100_000))));
    }

    [Fact]
    public void ANumberOrBooleanIsFineForATextField()
    {
        Assert.Null(Check(("Title", 42), ("Body", true)));
    }

    [Theory]
    [InlineData("not a date")]
    [InlineData("")]
    [InlineData("32/13/2026")]
    public void ADateThatIsNotADate_IsReported(string value)
    {
        Assert.Equal($"Field 'PublishDate' expects a date, but got '{value}'.", Check(("PublishDate", value)));
    }

    [Fact]
    public void ADateFieldDoesNotTakeANumberOrBoolean()
    {
        Assert.Equal("Field 'Starts' expects a date, but got 5.", Check(("Starts", 5)));
        Assert.Equal("Field 'Starts' expects a date, but got true.", Check(("Starts", true)));
    }

    [Theory]
    [InlineData("7")]
    [InlineData(7)]
    [InlineData(7L)]
    [InlineData(7.0)]
    public void WholeNumbersFitAnIntegerField(object value)
    {
        Assert.Null(Check(("Count", value)));
    }

    [Theory]
    [InlineData("seven")]
    [InlineData("")]
    [InlineData("1.5")]
    [InlineData(1.5)]
    [InlineData(3_000_000_000L)]
    [InlineData(true)]
    public void ThingsThatAreNotAnIntegerInRange_AreReported(object value)
    {
        Assert.StartsWith("Field 'Count' expects a whole number, but got ", Check(("Count", value)));
    }

    [Fact]
    public void ALongIntegerFieldTakesWhatAnIntegerFieldCannot()
    {
        Assert.Null(Check(("Big", 3_000_000_000L), ("Big", "3000000000")));
        Assert.StartsWith("Field 'Big' expects a whole number", Check(("Big", "9223372036854775808")));
    }

    [Theory]
    [InlineData("4.5")]
    [InlineData(4)]
    [InlineData(4.5)]
    public void NumbersFitADoubleAndADecimalField(object value)
    {
        Assert.Null(Check(("Rating", value), ("Price", value)));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(true)]
    public void ThingsThatAreNotANumber_AreReported(object value)
    {
        Assert.StartsWith("Field 'Price' expects a number, but got ", Check(("Price", value)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData("true")]
    [InlineData("False")]
    [InlineData(0)]
    [InlineData(1)]
    public void BooleanFieldsTakeTrueOrFalse(object value)
    {
        Assert.Null(Check(("Featured", value)));
    }

    [Theory]
    [InlineData("yes")]
    [InlineData("1")]
    [InlineData("")]
    [InlineData(2)]
    public void ThingsThatAreNotABoolean_AreReported(object value)
    {
        Assert.StartsWith("Field 'Featured' expects true or false, but got ", Check(("Featured", value)));
    }

    [Fact]
    public void ABadTimeSpanOrGuid_IsReported()
    {
        Assert.Equal("Field 'Length' expects a time span, but got 'long'.", Check(("Length", "long")));
        Assert.Equal("Field 'Key' expects a GUID, but got 'abc'.", Check(("Key", "abc")));
    }

    [Fact]
    public void EveryProblemIsReported_NotJustTheFirst()
    {
        var problem = FieldValueChecker.Check(
            Type,
            Rules,
            [KeyValuePair.Create<string, object?>("Title", "12345678901"), KeyValuePair.Create<string, object?>("PublishDate", "no"), KeyValuePair.Create<string, object?>("Nope", "x")],
            [new TypedField("Title", FieldUse.Tags)]);

        Assert.Equal(
            "Field 'Nope' was not found on content type 'Site.Article'. Field 'Title' allows at most 10 characters, but the value has 11. Field 'PublishDate' expects a date, but got 'no'. Field 'Title' is not a tag field (it is text), so it can't be set from TagFields.",
            problem);
    }

    [Fact]
    public void AReferenceFieldIsNotJudgedByItsValue()
    {
        Assert.Null(Check(("Related", "[{\"Identifier\":\"4faf9f31-e84c-4b6c-9015-02f358f1951f\"}]"), ("Related", 5)));
    }

    [Fact]
    public void ALongValueIsShortenedInTheMessage()
    {
        var problem = Check(("PublishDate", new string('x', 80)));

        Assert.Equal($"Field 'PublishDate' expects a date, but got '{new string('x', 50)}...'.", problem);
    }
}
