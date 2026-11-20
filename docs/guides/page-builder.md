# Work with Page Builder data

Page Builder lets editors lay a page out from sections and widgets. Relay can read that layout, change it, create a page that has one, and list the page templates a page can use. This guide follows pages of the Dancing Goat sample site. It makes sense on a site that uses Page Builder, because the templates and the components come from the site's code.

## What is stored

A page's Page Builder data has two parts, and Kentico stores both as JSON with the page's latest version:

| Part | What it holds |
|---|---|
| `widgets` | The layout. An `editableAreas` list; each area has `sections`, each section has widget `zones`, each zone has `widgets`, and each widget has `variants` whose `properties` are what the editor filled in. |
| `template` | The page template: its `identifier`, and its `properties` and `fieldIdentifiers` (`null` when nothing is set). |

Every section, zone, widget and variant has an `identifier` that is a GUID. A section and a widget also have a `type`, which names a component registered by the site's code, such as `Kentico.Widget.RichText`.

## 1. See which templates a page can use

[`get-page-templates`](../commands/web-pages/index.md#get-page-templates) lists the templates, the way the administration offers them to an editor. An article page of the sample site can use two:

```json
{ "webPageId": 9 }
```

```json
{
  "templates": [
    { "identifier": "DancingGoat.Article", "name": "Article detail", "contentTypeNames": [ "DancingGoat.ArticlePage" ] },
    { "identifier": "DancingGoat.ArticleWithSidebar", "name": "Article detail with sidebar", "contentTypeNames": [ "DancingGoat.ArticlePage" ] }
  ],
  "currentTemplate": "DancingGoat.Article"
}
```

(Abridged: each template also has a `description` and an `iconClass`.) `currentTemplate` is the one the page uses now. For a page that isn't created yet, give the content type and the parent it will go under instead: `{ "contentTypeName": "DancingGoat.LandingPage", "parentWebPageItemId": 4 }`.

## 2. Create a page with widgets and a template

[`create-web-page`](../commands/web-pages/index.md#create-web-page) takes the data in `pageBuilder`. This creates a landing page with one section, one zone and one rich text widget:

```json
{
  "websiteChannelName": "DancingGoatPages",
  "parentWebPageItemId": 4,
  "contentTypeName": "DancingGoat.LandingPage",
  "displayName": "Coffee tasting night",
  "publishAfterCreate": true,
  "pageBuilder": {
    "widgets": {
      "editableAreas": [
        {
          "identifier": "areaMain",
          "sections": [
            {
              "identifier": "5f4e6a0e-1b6c-4d6e-9a54-0e7a3d6b2c11",
              "type": "DancingGoat.SingleColumnSection",
              "properties": { "theme": null },
              "zones": [
                {
                  "identifier": "8d1d9c7a-0f56-43c4-8d0e-6a1c1b5f9a22",
                  "widgets": [
                    {
                      "identifier": "c1f3b6d2-71a5-4ad5-b0b4-3f1d2d4e5a33",
                      "type": "Kentico.Widget.RichText",
                      "variants": [
                        {
                          "identifier": "e2a4c7f3-82b6-4be6-81c5-4a2e3e5f6b44",
                          "properties": { "content": "<p>Join us on Friday for a tasting of three new roasts.</p>" }
                        }
                      ]
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    },
    "template": { "identifier": "DancingGoat.LandingPageSingleColumn", "properties": null, "fieldIdentifiers": null }
  }
}
```

The answer is `Created web page 'Coffee tasting night' (ID=68), published.` Make up the GUIDs yourself (`Guid.NewGuid()` in C#); they only have to be unique within the page.

## 3. Read it back

[`get-page`](../commands/web-pages/index.md#get-page) leaves the data out unless you ask for it, because the widgets of a page can be large:

```json
{ "webPageId": 68, "includePageBuilder": true }
```

`data.pageBuilder` then holds the `widgets` and the `template` exactly as stored, in the shape the request above used. A page that has no Page Builder data has a `null` `pageBuilder`.

## 4. Change a widget

[`update-web-page`](../commands/web-pages/index.md#update-web-page) replaces the part you send, so to change one widget you read the widgets, change what you need and send the whole structure back:

```json
{ "webPageId": 68, "pageBuilder": { "widgets": { "editableAreas": [ "...the whole structure, with the new text in the widget..." ] } } }
```

The answer is `Updated the Page Builder data of web page 68.` The edit goes through a draft like any other: a published page is published again with the change, and a draft or an unpublished page stays that way (see [Drafts and publishing](../concepts/drafts-and-publishing.md)). Widgets and template are separate. Send only one and the other stays as it is, and `fields` can go in the same call.

## 5. Change the template

```json
{
  "webPageId": 9,
  "pageBuilder": { "template": { "identifier": "DancingGoat.ArticleWithSidebar", "properties": null, "fieldIdentifiers": null } }
}
```

Only the template is sent, so the page's widgets stay. The template has to be one the page can use. The page's own template is always accepted, even if the site has stopped offering it.

## What Relay checks

Before anything is created or changed, Relay checks:

- that the widgets have the structure Kentico stores: areas, sections, zones, widgets and variants, each with an `identifier` that is a GUID and unused anywhere else in the data, and a `type` where one is needed;
- that the template has an `identifier` and is one the page can use;
- that each part is no longer than 2,000,000 characters.

Every problem is reported at once, with where it is, for example `Widgets.editableAreas[0].sections[0]: 'identifier' must be a GUID.` A request that fails a check changes nothing.

Relay does not check the `type` of a section or widget, nor the `properties` of a widget. Those belong to the site's components: a type the site doesn't register shows up in the editor as a missing component, so take the identifiers from the site's code (the default widgets are listed in [Kentico's reference](https://docs.kentico.com/documentation/developers-and-admins/development/builders/page-builder/reference-default-page-builder-widgets)).

## Which content items the widgets use

Kentico keeps a record of the content items that a page's widgets refer to. Relay writes through Kentico's own draft update, which keeps that record in step with the widgets: when a widget that picks content items is removed its items are dropped from the record, and when it comes back they are added again. There is nothing to send for it. Kentico works out which widget properties refer to content items from the components the site registers, so on a site whose code registers none the record isn't kept: another reason to write Page Builder data only on a site that uses Page Builder.

## The same in C\# { #the-same-in-csharp }

```csharp
using System.Text.Json;
using System.Text.Json.Nodes;
using Xperience.Relay.Client;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;

public class PageBuilderEditor(RelayClient relay)
{
    // Puts new text in every rich text widget of a page.
    public async Task ReplaceRichTextAsync(int pageId, string html)
    {
        var read = await relay.ExecuteAsync(new GetPageCommand { WebPageId = pageId, IncludePageBuilder = true });
        Check(read);

        var widgets = JsonNode.Parse(read.GetData<WebPageData>()!.PageBuilder!.Widgets!.Value.GetRawText())!;

        var richTextVariants = widgets["editableAreas"]!.AsArray()
            .SelectMany(area => area!["sections"]!.AsArray())
            .SelectMany(section => section!["zones"]!.AsArray())
            .SelectMany(zone => zone!["widgets"]!.AsArray())
            .Where(widget => (string?)widget!["type"] == "Kentico.Widget.RichText")
            .SelectMany(widget => widget!["variants"]!.AsArray());

        foreach (var variant in richTextVariants)
        {
            variant!["properties"]!["content"] = html;
        }

        Check(await relay.ExecuteAsync(new UpdateWebPageCommand
        {
            WebPageId = pageId,
            PageBuilder = new PageBuilderData { Widgets = JsonSerializer.Deserialize<JsonElement>(widgets.ToJsonString()) },
        }));
    }

    private static void Check(RelayCommandResult result)
    {
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error);
        }
    }
}
```

## What to read next

- [`get-page-templates`](../commands/web-pages/index.md#get-page-templates), for the templates of a page
- [`update-web-page`](../commands/web-pages/index.md#update-web-page) and [`create-web-page`](../commands/web-pages/index.md#create-web-page), for every parameter and example
- [Create, edit and publish a page](page-lifecycle.md), for the rest of a page's life
