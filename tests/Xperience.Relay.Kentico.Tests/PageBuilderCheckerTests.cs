using System.Text.Json;
using Xperience.Relay.Contracts;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Tests;

public class PageBuilderCheckerTests
{
    private const string Section = "5f4e6a0e-1b6c-4d6e-9a54-0e7a3d6b2c11";
    private const string Zone = "8d1d9c7a-0f56-43c4-8d0e-6a1c1b5f9a22";
    private const string Widget = "c1f3b6d2-71a5-4ad5-b0b4-3f1d2d4e5a33";
    private const string Variant = "e2a4c7f3-82b6-4be6-81c5-4a2e3e5f6b44";

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static string Widgets(string section = Section, string zone = Zone, string widget = Widget, string variant = Variant) => $$$"""
        {"editableAreas":[{"identifier":"areaMain","sections":[{"identifier":"{{{section}}}","type":"DancingGoat.SingleColumnSection","properties":{"theme":null},
        "zones":[{"identifier":"{{{zone}}}","widgets":[{"identifier":"{{{widget}}}","type":"Kentico.Widget.RichText",
        "variants":[{"identifier":"{{{variant}}}","properties":{"content":"<p>Hello</p>"},"fieldIdentifiers":{"content":"2d4c3a5e-6a7b-4c8d-9e0f-1a2b3c4d5e6f"}}]}]}]}]}]}
        """;

    private static string? Check(string? widgets = null, string? template = null) => PageBuilderChecker.Check(new PageBuilderData
    {
        Widgets = widgets is null ? null : Json(widgets),
        Template = template is null ? null : Json(template),
    });

    [Fact]
    public void WidgetsAsKenticoStoresThem_Pass()
    {
        Assert.Null(Check(widgets: Widgets()));
    }

    [Fact]
    public void AnEmptyPage_Passes()
    {
        Assert.Null(Check(widgets: """{"editableAreas":[]}"""));
        Assert.Null(Check(widgets: """{"editableAreas":[{"identifier":"areaMain","sections":[]}]}"""));
    }

    [Theory]
    [InlineData("""{"identifier":"DancingGoat.Article"}""")]
    [InlineData("""{"identifier":"DancingGoat.Article","properties":null,"fieldIdentifiers":null}""")]
    [InlineData("""{"identifier":"DancingGoat.Article","properties":{"showTitle":true}}""")]
    public void ATemplateWithAnIdentifier_Passes(string template)
    {
        Assert.Null(Check(template: template));
    }

    [Fact]
    public void NothingToSet_IsRefused()
    {
        Assert.Equal("PageBuilder needs Widgets, Template or both.", PageBuilderChecker.Check(new PageBuilderData()));
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    [InlineData("{}")]
    [InlineData("""{"editableAreas":{}}""")]
    [InlineData("""{"EditableAreas":[]}""")]
    public void WidgetsWithoutAListOfEditableAreas_AreRefused(string widgets)
    {
        Assert.Equal("PageBuilder.Widgets must be an object with an 'editableAreas' list.", Check(widgets: widgets));
    }

    [Fact]
    public void ProblemsAreReported_WithWhereTheyAre()
    {
        var widgets = """
            {"editableAreas":[{"sections":[{"identifier":"x","zones":[{"identifier":"y","widgets":[{"identifier":"z","variants":[]}]}]}]}]}
            """;

        var problem = Check(widgets: widgets);

        Assert.Equal(
            "Widgets.editableAreas[0]: 'identifier' is required. "
            + "Widgets.editableAreas[0].sections[0]: 'identifier' must be a GUID. "
            + "Widgets.editableAreas[0].sections[0]: 'type' is required. "
            + "Widgets.editableAreas[0].sections[0].zones[0]: 'identifier' must be a GUID. "
            + "Widgets.editableAreas[0].sections[0].zones[0].widgets[0]: 'identifier' must be a GUID. "
            + "Widgets.editableAreas[0].sections[0].zones[0].widgets[0]: 'type' is required. "
            + "Widgets.editableAreas[0].sections[0].zones[0].widgets[0]: 'variants' needs at least one variant.",
            problem);
    }

    [Fact]
    public void ALevelThatIsNotAnObjectOrAList_IsRefused()
    {
        Assert.Equal(
            "Widgets.editableAreas[0] must be an object.",
            Check(widgets: """{"editableAreas":[1]}"""));

        Assert.Equal(
            "Widgets.editableAreas[0]: 'sections' must be a list.",
            Check(widgets: """{"editableAreas":[{"identifier":"a"}]}"""));
    }

    [Fact]
    public void ABadVariant_IsRefused()
    {
        var widgets = Widgets(variant: "not-a-guid");

        Assert.Equal(
            "Widgets.editableAreas[0].sections[0].zones[0].widgets[0].variants[0]: 'identifier' must be a GUID.",
            Check(widgets: widgets));
    }

    [Fact]
    public void PropertiesThatAreNotAnObject_AreRefused()
    {
        var widgets = Widgets().Replace("\"properties\":{\"content\":\"<p>Hello</p>\"}", "\"properties\":\"text\"");

        Assert.Equal(
            "Widgets.editableAreas[0].sections[0].zones[0].widgets[0].variants[0]: 'properties' must be an object or null.",
            Check(widgets: widgets));
    }

    [Fact]
    public void AnIdentifierUsedTwice_IsRefused_WhateverItsCase()
    {
        var widgets = Widgets(zone: Section.ToUpperInvariant());

        var problem = Check(widgets: widgets);

        Assert.Equal(
            $"Widgets.editableAreas[0].sections[0].zones[0]: the identifier {Section} is used more than once.",
            problem);
    }

    [Fact]
    public void OnlyTheFirstProblems_AreShown()
    {
        var sections = string.Join(",", Enumerable.Repeat("""{"identifier":"x","type":"S","zones":[]}""", 12));
        var problem = Check(widgets: $$"""{"editableAreas":[{"identifier":"a","sections":[{{sections}}]}]}""")!;

        Assert.Equal(10, problem.Split("'identifier' must be a GUID.").Length - 1);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("\"DancingGoat.Article\"")]
    public void ATemplateThatIsNotAnObject_IsRefused(string template)
    {
        Assert.Equal("PageBuilder.Template must be an object with the template's 'identifier'.", Check(template: template));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"identifier":""}""")]
    [InlineData("""{"identifier":5}""")]
    public void ATemplateWithoutAnIdentifier_IsRefused(string template)
    {
        Assert.Equal("PageBuilder.Template: 'identifier' is required.", Check(template: template));
    }

    [Fact]
    public void ATemplateWithPropertiesThatAreNotAnObject_IsRefused()
    {
        Assert.Equal(
            "Template: 'properties' must be an object or null.",
            Check(template: """{"identifier":"DancingGoat.Article","properties":[1]}"""));
    }

    [Fact]
    public void JsonOverTheLimit_IsRefused()
    {
        var text = new string('a', PageBuilderChecker.MaxJsonLength);

        Assert.Equal(
            "PageBuilder.Widgets is longer than 2000000 characters.",
            Check(widgets: $$"""{"editableAreas":[],"note":"{{text}}"}"""));

        Assert.Equal(
            "PageBuilder.Template is longer than 2000000 characters.",
            Check(template: $$"""{"identifier":"x","note":"{{text}}"}"""));
    }

    [Fact]
    public void BothPartsAreChecked_AndBothProblemsReported()
    {
        var problem = Check(widgets: "[]", template: "[]");

        Assert.Equal(
            "PageBuilder.Widgets must be an object with an 'editableAreas' list. PageBuilder.Template must be an object with the template's 'identifier'.",
            problem);
    }

    [Fact]
    public void TheTemplateIdentifier_IsReadFromTheTemplate()
    {
        Assert.Equal("DancingGoat.Article", PageBuilderChecker.TemplateIdentifier(Json("""{"identifier":"DancingGoat.Article","properties":null}""")));
        Assert.Null(PageBuilderChecker.TemplateIdentifier(Json("{}")));
        Assert.Null(PageBuilderChecker.TemplateIdentifier(Json("[]")));
        Assert.Null(PageBuilderChecker.TemplateIdentifier(null));
    }

    [Fact]
    public void StoredJson_BecomesResultData()
    {
        var data = new StoredPageBuilder("""{"editableAreas":[]}""", """{"identifier":"DancingGoat.Article"}""").ToData();

        Assert.NotNull(data);
        Assert.Equal(JsonValueKind.Object, data.Widgets!.Value.ValueKind);
        Assert.Equal("DancingGoat.Article", PageBuilderChecker.TemplateIdentifier(data.Template));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "  ")]
    [InlineData("null", "null")]
    [InlineData("{not json", "also not json")]
    public void NothingUsable_IsNoData(string? widgets, string? template)
    {
        Assert.Null(new StoredPageBuilder(widgets, template).ToData());
    }

    [Fact]
    public void APartThatIsNotJson_IsLeftOut_NotTheOtherPart()
    {
        var data = new StoredPageBuilder("{not json", """{"identifier":"DancingGoat.Article"}""").ToData();

        Assert.NotNull(data);
        Assert.Null(data.Widgets);
        Assert.NotNull(data.Template);
    }
}
