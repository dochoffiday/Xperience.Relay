---
toc_depth: 2
---

# Web pages

Pages live in the content tree of a website channel. Every page command takes the page's ID (`webPageId`): [`create-web-page`](#create-web-page) returns it, and [`get-page-info`](#get-page-info) or [`query-web-page-items`](#query-web-page-items) find it for pages that already exist.

A page has one variant per language, and publishing is per variant. Changes go through drafts the way they do in the admin UI: see [Drafts and publishing](../../concepts/drafts-and-publishing.md). [Create, edit and publish a page](../../guides/page-lifecycle.md) walks through the commands in order.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`create-web-page`](#create-web-page) | Write | Creates a new web page under a parent web page. |
| [`update-web-page`](#update-web-page) | Write | Updates one or more fields on an existing web page. |
| [`update-slug`](#update-slug) | Write | Changes the URL slug of a web page. |
| [`move-web-page`](#move-web-page) | Write | Moves an existing web page under a different parent, or to the root of its channel. |
| [`publish-web-page`](#publish-web-page) | Write | Publishes a web page language variant: a draft is published, and an unpublished page gets a draft made first. |
| [`unpublish-web-page`](#unpublish-web-page) | Write | Unpublishes a web page language variant. |
| [`delete-web-page`](#delete-web-page) | Write | Deletes a language variant of a web page. |
| [`get-page-info`](#get-page-info) | Read | Fetches a web page's system fields (content type, tree path, parent ID, ...) without its content-type-specific field data. |
| [`get-page`](#get-page) | Read | Fetches a web page's system fields plus the field values of its latest version (drafts included). |
| [`query-web-page-items`](#query-web-page-items) | Read | Queries web pages of one or more content types, returning only the requested columns. |
<!-- relay:end commands -->

---

## create-web-page

<!-- relay:begin header:create-web-page -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-web-page` | [Write](../../concepts/access-tiers.md) | [`CreateWebPageResult`](../../reference/types.md#createwebpageresult) | `CreateWebPageCommand` |
<!-- relay:end header:create-web-page -->

Creates a web page under a parent page, in one language, with the field values you give it. The page starts as a draft; set `publishAfterCreate` to publish it straight away.

Everything is checked before anything is created. The content type has to exist, every field has to be a field of that type, and every value has to fit its field, so a request that fails a check creates nothing.

<!-- relay:begin parameters:create-web-page -->
### Parameters { #create-web-page-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `websiteChannelName` | string | No | Website channel that owns the page. Defaults to [`RelayKenticoOptions.DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname) when null. |
| `parentWebPageItemId` | integer | No | ID of the parent web page, or 0 (the default) for the root of the channel. The parent's content type has to allow the new page's content type as a child. |
| `contentTypeName` | string | Yes | Code name of the page's content type, e.g. `DancingGoat.ArticlePage`. |
| `displayName` | string | Yes | Display name of the new page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `urlSlug` | string | No | Optional URL slug. When null, Kentico generates one from the display name. |
| `fields` | { name: any } | No | Plain field values by field name (text, numbers, booleans, dates, ...). |
| `linkedItemFields` | { name: guid[] } | No | Linked item fields -- field name to list of content item GUIDs. The field must be a content item reference field. |
| `tagFields` | { name: guid[] } | No | Tag fields -- field name to list of tag GUIDs. The field must be a tag (taxonomy) field. |
| `publishAfterCreate` | boolean | No | When true, publishes the page immediately after creation. Defaults to false. |
| `assets` | [RelayAsset](../../reference/types.md#relayasset)[] | No | Optional binary assets to upload into asset fields. Each entry maps to one field. |
<!-- relay:end parameters:create-web-page -->

<!-- relay:begin result:create-web-page -->
### Result { #create-web-page-result }

A successful run returns a [CreateWebPageResult](../../reference/types.md#createwebpageresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `webPageItemId` | integer | ID of the new page, for the other page commands. |
| `webPageItemGuid` | guid | GUID of the new page. |
<!-- relay:end result:create-web-page -->

### Behavior { #create-web-page-behavior }

- **State.** The page is created as an `InitialDraft`, which is not on the live site yet. With `publishAfterCreate` set to `true` it is published right after it is created. If Kentico creates the page but then declines to publish it, the result is a failure that names the new page's ID, so a retry doesn't create a second page. See [Drafts and publishing](../../concepts/drafts-and-publishing.md).
- **Channel.** `websiteChannelName` is the channel's code name, not its display name. When it is left out Relay uses [`DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname), and the command fails if that isn't set either.
- **Parent.** `parentWebPageItemId` is the ID of the parent page; find it with [`get-page-info`](#get-page-info) or [`query-web-page-items`](#query-web-page-items). `0`, the default, creates the page at the root of the channel. Kentico decides what can go where: the parent's content type has to allow the new page's content type as a child, otherwise Kentico refuses with "Content type with ID ... is not allowed as a child of web page with ID ...".
- **URL slug.** When `urlSlug` is left out Kentico makes one from the display name. Change it later with [`update-slug`](#update-slug).
- **Fields.** `fields` takes plain values: text, numbers, booleans and dates. Field names are not case sensitive. A field the content type doesn't have is refused (Kentico would drop it without saying so), and a value has to fit its field: a whole number for an integer field, a date that parses for a date field, text no longer than the field allows, and so on. Every problem found is reported in one answer. Fields that come from a reusable field schema count as fields of the type.
- **Linked items, tags and files.** `linkedItemFields` takes content item GUIDs for content item reference fields, `tagFields` takes tag GUIDs for tag fields, and `assets` takes Base64 files for asset fields. Each one only goes into a field of that kind. See [Upload files](../../guides/uploading-assets.md).
- **Language.** The page is created in `languageName`, or in [`DefaultLanguageName`](../../configuration.md#defaultlanguagename). Relay has no command that adds another language to an existing page.
- **The result.** `webPageItemId` is the ID that the other page commands call `webPageId`.

<!-- relay:begin examples:create-web-page -->
### Examples { #create-web-page-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Create an article as a draft { #create-web-page-example-draft-article }

The parent (`parentWebPageItemId` 1) is the Articles section of the Dancing Goat sample site. The page starts as an `InitialDraft`.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-web-page",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "parentWebPageItemId": 1,
        "contentTypeName": "DancingGoat.ArticlePage",
        "displayName": "Cold brew at home",
        "fields": {
          "ArticleTitle": "Cold brew at home",
          "ArticlePageSummary": "Steep coarse grounds in cold water overnight for a smooth, low-acid coffee.",
          "ArticlePageText": "<p>Mix 100 g of coarse grounds with 1 litre of cold water and leave it for 12 hours.</p>",
          "ArticlePagePublishDate": "2026-10-01"
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateWebPageCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ParentWebPageItemId = 1,
        ContentTypeName = "DancingGoat.ArticlePage",
        DisplayName = "Cold brew at home",
        Fields = new()
        {
            ["ArticleTitle"] = JsonSerializer.SerializeToElement("Cold brew at home"),
            ["ArticlePageSummary"] = JsonSerializer.SerializeToElement("Steep coarse grounds in cold water overnight for a smooth, low-acid coffee."),
            ["ArticlePageText"] = JsonSerializer.SerializeToElement("<p>Mix 100 g of coarse grounds with 1 litre of cold water and leave it for 12 hours.</p>"),
            ["ArticlePagePublishDate"] = JsonSerializer.SerializeToElement("2026-10-01"),
        },
    });
    var data = result.GetData<CreateWebPageResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created web page 'Cold brew at home' (ID=68), draft.",
  "error": null,
  "data": {
    "webPageItemId": 68,
    "webPageItemGuid": "5b308d07-cbb6-4ccb-8d85-6068cc689e17"
  }
}
```

#### Create a page, set its URL slug and publish it straight away { #create-web-page-example-published-with-slug }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-web-page",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "parentWebPageItemId": 1,
        "contentTypeName": "DancingGoat.ArticlePage",
        "displayName": "Brewing methods compared",
        "urlSlug": "brewing-methods-compared",
        "publishAfterCreate": true,
        "fields": {
          "ArticleTitle": "Brewing methods compared",
          "ArticlePageSummary": "Pour-over, French press or AeroPress? What each one does to the cup."
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateWebPageCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ParentWebPageItemId = 1,
        ContentTypeName = "DancingGoat.ArticlePage",
        DisplayName = "Brewing methods compared",
        UrlSlug = "brewing-methods-compared",
        PublishAfterCreate = true,
        Fields = new()
        {
            ["ArticleTitle"] = JsonSerializer.SerializeToElement("Brewing methods compared"),
            ["ArticlePageSummary"] = JsonSerializer.SerializeToElement("Pour-over, French press or AeroPress? What each one does to the cup."),
        },
    });
    var data = result.GetData<CreateWebPageResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created web page 'Brewing methods compared' (ID=69), published.",
  "error": null,
  "data": {
    "webPageItemId": 69,
    "webPageItemGuid": "41dca4c0-2931-4949-b905-c5def7b31a8c"
  }
}
```
<!-- relay:end examples:create-web-page -->

<!-- relay:begin errors:create-web-page -->
### Errors { #create-web-page-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A content type that is not allowed under the parent"

    The root of the channel only allows the content types the channel permits there; an article page is not one of them.

    Parameters:

    ```json
    {
      "websiteChannelName": "DancingGoatPages",
      "parentWebPageItemId": 0,
      "contentTypeName": "DancingGoat.ArticlePage",
      "displayName": "Cold brew at the root"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Unhandled exception executing verb 'create-web-page': [InvalidOperationException] Content type with ID 5665 is not allowed as a child of web page with ID 0.",
      "data": null
    }
    ```

???+ failure "Fields the content type does not have, or values that do not fit"

    Parameters:

    ```json
    {
      "websiteChannelName": "DancingGoatPages",
      "parentWebPageItemId": 1,
      "contentTypeName": "DancingGoat.ArticlePage",
      "displayName": "Cold brew at home",
      "fields": {
        "ArticleSubtitle": "A field this type does not have",
        "ArticlePagePublishDate": "last Tuesday"
      }
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Field 'ArticleSubtitle' was not found on content type 'DancingGoat.ArticlePage'. Field 'ArticlePagePublishDate' expects a date, but got 'last Tuesday'.",
      "data": null
    }
    ```

???+ failure "No website channel named, and no default configured"

    Parameters:

    ```json
    {
      "parentWebPageItemId": 1,
      "contentTypeName": "DancingGoat.ArticlePage",
      "displayName": "Cold brew at home"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "WebsiteChannelName is required. Set it on the command or configure RelayKenticoOptions.DefaultWebsiteChannelName.",
      "data": null
    }
    ```
<!-- relay:end errors:create-web-page -->

### See also { #create-web-page-see-also }

- [`update-web-page`](#update-web-page) to change the fields later
- [`publish-web-page`](#publish-web-page) to publish a draft
- [`get-page-info`](#get-page-info) to look up the parent's ID
- [Create, edit and publish a page](../../guides/page-lifecycle.md)

---

## update-web-page

<!-- relay:begin header:update-web-page -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateWebPageCommand` |
<!-- relay:end header:update-web-page -->

Changes fields of a page in one language. Relay edits through a draft, the way the admin UI does, and leaves the page in the state it found it in: a page that was published is published again with the change, and a page that was a draft or unpublished stays that way.

<!-- relay:begin parameters:update-web-page -->
### Parameters { #update-web-page-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `fields` | { name: any } | No | Plain field values by field name (text, numbers, booleans, dates, ...). |
| `linkedItemFields` | { name: guid[] } | No | Linked-items fields. Maps field name to a list of ContentItemGUIDs. The field must be a content item reference field. An empty list clears the field. |
| `tagFields` | { name: guid[] } | No | Tag fields. Maps field name to a list of tag GUIDs. The field must be a tag (taxonomy) field. An empty list clears the field. |
| `assets` | [RelayAsset](../../reference/types.md#relayasset)[] | No | Optional binary assets to upload into asset fields. Each entry maps to one field. |
<!-- relay:end parameters:update-web-page -->

<!-- relay:begin result:update-web-page -->
### Result { #update-web-page-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:update-web-page -->

### Behavior { #update-web-page-behavior }

What happens to the page depends on its latest version:

| The page is | Relay | Afterwards |
|---|---|---|
| Published | makes a draft, saves the change, publishes it | Published, with the change |
| Published, with a newer draft | saves the change into that draft | Still a draft over the published version; the live page is unchanged |
| Never published | saves the change into its draft | Still never published |
| Unpublished | makes a draft and saves the change into it | Still not published |

- **The answer says when the change isn't live.** A page that isn't going live ends the message with "Saved as a draft; it is not published." or, when a published version exists, "Saved as a draft; the published version is unchanged."
- **One language.** Relay reads the page's state in `languageName` first. A page that has no variant in that language fails with "was not found in that language", and other languages are never touched.
- **Failures leave nothing behind.** If Kentico declines to create the draft, to save the change or to publish it, the result says which step failed. A draft made for an edit that then failed is discarded again (a draft that was already there is kept as it was), and Kentico's cached copy of the page is dropped so the next edit doesn't write the refused value again. If only the final publish fails, the change stays saved as a draft and the live page is unchanged. Relay never reports a success it didn't get.
- **Checks.** The fields are checked the way [`create-web-page`](#create-web-page) checks them, before the draft is made, so a bad request changes nothing.
- **Clearing a field.** An empty list in `linkedItemFields` or `tagFields` clears that field. `null` in `fields` is refused for a field that must have a value.
- **Something to change.** At least one of `fields`, `linkedItemFields`, `tagFields` and `assets` must be given, otherwise the answer is "No fields provided — nothing to update."
- **The URL.** This command doesn't change the page's URL slug; use [`update-slug`](#update-slug).

<!-- relay:begin examples:update-web-page -->
### Examples { #update-web-page-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Update fields on a page that has never been published { #update-web-page-example-draft }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-web-page",
      "parameters": {
        "webPageId": 68,
        "fields": {
          "ArticleTitle": "Cold brew at home: a beginner'\''s guide",
          "ArticlePageSummary": "Everything you need to make cold brew with a jar and a sieve."
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateWebPageCommand
    {
        WebPageId = 68,
        Fields = new()
        {
            ["ArticleTitle"] = JsonSerializer.SerializeToElement("Cold brew at home: a beginner's guide"),
            ["ArticlePageSummary"] = JsonSerializer.SerializeToElement("Everything you need to make cold brew with a jar and a sieve."),
        },
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated 2 field(s) on web page 68. Saved as a draft; it is not published.",
  "error": null,
  "data": null
}
```

#### Update a published page { #update-web-page-example-published }

The page was published, so Relay makes a draft, saves the change and publishes it again.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-web-page",
      "parameters": {
        "webPageId": 68,
        "fields": {
          "ArticlePageSummary": "Everything you need to make cold brew with a jar and a sieve - no special equipment."
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateWebPageCommand
    {
        WebPageId = 68,
        Fields = new()
        {
            ["ArticlePageSummary"] = JsonSerializer.SerializeToElement("Everything you need to make cold brew with a jar and a sieve - no special equipment."),
        },
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated 1 field(s) on web page 68.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:update-web-page -->

<!-- relay:begin errors:update-web-page -->
### Errors { #update-web-page-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A field the content type does not have, and a value that does not fit"

    Parameters:

    ```json
    {
      "webPageId": 68,
      "fields": {
        "ArticleSubtitle": "A field this type does not have",
        "ArticlePagePublishDate": "last Tuesday"
      }
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Field 'ArticleSubtitle' was not found on content type 'DancingGoat.ArticlePage'. Field 'ArticlePagePublishDate' expects a date, but got 'last Tuesday'.",
      "data": null
    }
    ```
<!-- relay:end errors:update-web-page -->

### See also { #update-web-page-see-also }

- [`update-slug`](#update-slug) to change the URL
- [`publish-web-page`](#publish-web-page) to put a draft live
- [`get-page`](#get-page) to read the fields back
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)

---

## update-slug

<!-- relay:begin header:update-slug -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-slug` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateSlugCommand` |
<!-- relay:end header:update-slug -->

Changes a page's URL slug, the last part of its URL path, in one language.

<!-- relay:begin parameters:update-slug -->
### Parameters { #update-slug-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `slug` | string | Yes | The new slug (the last part of the URL path). |
<!-- relay:end parameters:update-slug -->

<!-- relay:begin result:update-slug -->
### Result { #update-slug-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:update-slug -->

### Behavior { #update-slug-behavior }

- **Same flow as [`update-web-page`](#update-web-page).** A published page is published again, so the new URL is live as soon as the answer comes back. A page that is a draft or unpublished keeps the new slug in its draft, and the answer says it isn't live. See [Drafts and publishing](../../concepts/drafts-and-publishing.md).
- **What the slug is.** It is the page's own part of the URL, after the parts of its parents: with the slug `cold-brew-guide`, the article under Articles is served at `/articles/cold-brew-guide`. The page's tree path (`/Articles/Cold_brew_at_home`) does not change.
- **One language.** Only `languageName` changes; the page keeps its URLs in other languages.
- **Checking the result.** [`query-web-page-items`](#query-web-page-items) with `includeUrls` shows the URL a page has now.

<!-- relay:begin examples:update-slug -->
### Examples { #update-slug-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Change the URL slug of a published page { #update-slug-example-published }

The page was published, so the new slug is published too and goes live immediately.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-slug",
      "parameters": {
        "webPageId": 68,
        "slug": "cold-brew-guide"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateSlugCommand
    {
        WebPageId = 68,
        Slug = "cold-brew-guide",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated slug on web page 68 to 'cold-brew-guide'.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:update-slug -->

<!-- relay:begin errors:update-slug -->
<!-- relay:end errors:update-slug -->

### See also { #update-slug-see-also }

- [`update-web-page`](#update-web-page) to change the fields
- [`query-web-page-items`](#query-web-page-items) to see URLs
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)

---

## move-web-page

<!-- relay:begin header:move-web-page -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `move-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `MoveWebPageCommand` |
<!-- relay:end header:move-web-page -->

Moves a page, together with the pages below it, under a different parent, or to the root of the channel.

<!-- relay:begin parameters:move-web-page -->
### Parameters { #move-web-page-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the page to move. |
| `parentWebPageId` | integer | Yes | ID of the new parent page, or 0 for the root of the channel. |
<!-- relay:end parameters:move-web-page -->

<!-- relay:begin result:move-web-page -->
### Result { #move-web-page-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:move-web-page -->

### Behavior { #move-web-page-behavior }

- **The parent has to exist.** `parentWebPageId` is the ID of the new parent page, or `0` for the root of the channel. A parent that doesn't exist fails with "Web page N (the new parent) was not found." Find IDs with [`get-page-info`](#get-page-info) or [`query-web-page-items`](#query-web-page-items).
- **Kentico decides what may go where.** The parent's content type has to allow the page's content type as a child; otherwise Kentico refuses with "Content type with ID ... is not allowed as a child of web page with ID ...".
- **Position.** Relay doesn't set a sibling order. Kentico puts the moved page first among its new siblings.
- **Immediate.** A move takes effect at once; there is no draft to publish. [`get-page-info`](#get-page-info) shows the new `treePath` and `parentWebPageId`.

<!-- relay:begin examples:move-web-page -->
### Examples { #move-web-page-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Move a navigation item out of the Store menu { #move-web-page-example-navigation-item }

Page 25 is `/Navigation_menu/Store/Merchandise` and page 5 is `/Navigation_menu`. Kentico puts the moved page first among its new siblings.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "move-web-page",
      "parameters": {
        "webPageId": 25,
        "parentWebPageId": 5
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new MoveWebPageCommand
    {
        WebPageId = 25,
        ParentWebPageId = 5,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Moved web page 25 under parent 5.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:move-web-page -->

<!-- relay:begin errors:move-web-page -->
### Errors { #move-web-page-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A parent that does not allow the page's content type"

    Parameters:

    ```json
    {
      "webPageId": 68,
      "parentWebPageId": 69
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Unhandled exception executing verb 'move-web-page': [InvalidOperationException] Content type with ID 5665 is not allowed as a child of web page with ID 69.",
      "data": null
    }
    ```

???+ failure "A parent that does not exist"

    Parameters:

    ```json
    {
      "webPageId": 68,
      "parentWebPageId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Web page 99999 (the new parent) was not found.",
      "data": null
    }
    ```
<!-- relay:end errors:move-web-page -->

### See also { #move-web-page-see-also }

- [`get-page-info`](#get-page-info) to look up IDs and the tree path
- [`create-web-page`](#create-web-page)
- [`delete-web-page`](#delete-web-page)

---

## publish-web-page

<!-- relay:begin header:publish-web-page -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `publish-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `PublishWebPageCommand` |
<!-- relay:end header:publish-web-page -->

Publishes a page in one language.

<!-- relay:begin parameters:publish-web-page -->
### Parameters { #publish-web-page-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:publish-web-page -->

<!-- relay:begin result:publish-web-page -->
### Result { #publish-web-page-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:publish-web-page -->

### Behavior { #publish-web-page-behavior }

What happens depends on the page's latest version:

| Latest version | What Relay does |
|---|---|
| A draft (including a draft over a published version) | Publishes the draft |
| Unpublished | Makes a draft first, then publishes it, so an unpublished page can be published again |
| Published | Nothing; the answer says the page is already published |

- **Safe to repeat.** Asking for a state the page is already in succeeds without changing anything.
- **Never a false answer.** A page, or a variant in that language, that doesn't exist is a failure, and so is Kentico declining to publish ("Kentico did not publish ... Check the event log.").
- **One language.** Only `languageName` is published.

<!-- relay:begin examples:publish-web-page -->
### Examples { #publish-web-page-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Publish a page that is a draft { #publish-web-page-example-draft }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-web-page",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishWebPageCommand
    {
        WebPageId = 68,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Published web page 68 (en).",
  "error": null,
  "data": null
}
```

#### Publish a page that is already published { #publish-web-page-example-already-published }

Asking for a state the page is already in succeeds without changing anything, so the command is safe to repeat.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-web-page",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishWebPageCommand
    {
        WebPageId = 68,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Web page 68 (en) is already published.",
  "error": null,
  "data": null
}
```

#### Publish a page that was unpublished { #publish-web-page-example-unpublished }

An unpublished page gets a draft first, then that draft is published.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-web-page",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishWebPageCommand
    {
        WebPageId = 68,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Published web page 68 (en).",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:publish-web-page -->

<!-- relay:begin errors:publish-web-page -->
<!-- relay:end errors:publish-web-page -->

### See also { #publish-web-page-see-also }

- [`unpublish-web-page`](#unpublish-web-page) to take a page offline
- [`update-web-page`](#update-web-page) to change a page without publishing it
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)

---

## unpublish-web-page

<!-- relay:begin header:unpublish-web-page -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `unpublish-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UnpublishWebPageCommand` |
<!-- relay:end header:unpublish-web-page -->

Takes a published page in one language offline.

<!-- relay:begin parameters:unpublish-web-page -->
### Parameters { #unpublish-web-page-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:unpublish-web-page -->

<!-- relay:begin result:unpublish-web-page -->
### Result { #unpublish-web-page-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:unpublish-web-page -->

### Behavior { #unpublish-web-page-behavior }

What happens depends on the page's latest version:

| Latest version | What Relay does |
|---|---|
| Published | Unpublishes it |
| Unpublished | Nothing; the answer says it is already unpublished |
| A draft that was never published | Nothing; the answer says the page isn't published and only has a draft |
| Published, with a newer draft | Refuses: Kentico won't unpublish a page while it has a draft with unpublished changes. Publish or discard the draft first |

- **The content stays.** An unpublished page isn't deleted; [`publish-web-page`](#publish-web-page) puts it back online.
- **Safe to repeat.** Asking for a state the page is already in succeeds without changing anything.
- **One language.** Only `languageName` is unpublished.

<!-- relay:begin examples:unpublish-web-page -->
### Examples { #unpublish-web-page-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Take a published page offline { #unpublish-web-page-example-published }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "unpublish-web-page",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UnpublishWebPageCommand
    {
        WebPageId = 68,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Unpublished web page 68 (en).",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:unpublish-web-page -->

<!-- relay:begin errors:unpublish-web-page -->
<!-- relay:end errors:unpublish-web-page -->

### See also { #unpublish-web-page-see-also }

- [`publish-web-page`](#publish-web-page)
- [`delete-web-page`](#delete-web-page) to remove a page instead
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)

---

## delete-web-page

<!-- relay:begin header:delete-web-page -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteWebPageCommand` |
<!-- relay:end header:delete-web-page -->

Deletes a page in one language. Unless you ask for `permanently`, the page goes to the recycle bin, where it can be restored from the admin UI.

<!-- relay:begin parameters:delete-web-page -->
### Parameters { #delete-web-page-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `redirectToWebPageId` | integer | No | When set, a redirect is created pointing to this web page after deletion. |
| `permanently` | boolean | No | Bypasses the recycle bin and permanently removes the page. Defaults to false. |
<!-- relay:end parameters:delete-web-page -->

<!-- relay:begin result:delete-web-page -->
### Result { #delete-web-page-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:delete-web-page -->

### Behavior { #delete-web-page-behavior }

- **Recycle bin, or gone for good.** By default the page and the pages below it are sent to the recycle bin. With `permanently: true` they are removed for good, and there is no way back.
- **The pages below it go too.** Deleting a page deletes its child pages with it.
- **The language.** The command deletes the language variant you name. When that is the page's only variant, the page itself is deleted; a page that also has other languages keeps those.
- **Redirect.** `redirectToWebPageId` asks Kentico to redirect visitors of the deleted page's URLs to another page. The target page has to exist.
- **Missing pages.** A page that doesn't exist, or has no variant in `languageName`, fails. Relay never reports a deletion that didn't happen.

<!-- relay:begin examples:delete-web-page -->
### Examples { #delete-web-page-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Delete a page (it goes to the recycle bin) { #delete-web-page-example-recycle-bin }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-web-page",
      "parameters": {
        "webPageId": 68,
        "languageName": "en"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteWebPageCommand
    {
        WebPageId = 68,
        LanguageName = "en",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted web page 68 (en).",
  "error": null,
  "data": null
}
```

#### Delete a page for good { #delete-web-page-example-permanently }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-web-page",
      "parameters": {
        "webPageId": 69,
        "permanently": true
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteWebPageCommand
    {
        WebPageId = 69,
        Permanently = true,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted web page 69 (en) permanently.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:delete-web-page -->

<!-- relay:begin errors:delete-web-page -->
### Errors { #delete-web-page-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A page that does not exist"

    Parameters:

    ```json
    {
      "webPageId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Web page 99999 was not found.",
      "data": null
    }
    ```

???+ failure "A language the page has no variant in"

    Page 9 exists, but only in English.

    Parameters:

    ```json
    {
      "webPageId": 9,
      "languageName": "es"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Web page 9 (es) was not found in that language.",
      "data": null
    }
    ```
<!-- relay:end errors:delete-web-page -->

### See also { #delete-web-page-see-also }

- [`unpublish-web-page`](#unpublish-web-page) to take a page offline without deleting it
- [`move-web-page`](#move-web-page)
- [`get-page-info`](#get-page-info) to check a page exists

---

## get-page-info

<!-- relay:begin header:get-page-info -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-page-info` | [Read](../../concepts/access-tiers.md) | [`WebPageInfo`](../../reference/types.md#webpageinfo) | `GetPageInfoCommand` |
<!-- relay:end header:get-page-info -->

Looks up a page's system fields: its content type, its place in the content tree, its parent and its channel. It doesn't return field values, so it is the cheap way to check that a page exists, or to turn a page into the IDs and paths other commands need.

<!-- relay:begin parameters:get-page-info -->
### Parameters { #get-page-info-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:get-page-info -->

<!-- relay:begin result:get-page-info -->
### Result { #get-page-info-result }

A successful run returns a [WebPageInfo](../../reference/types.md#webpageinfo) in `data`:

| Field | Type | Description |
|---|---|---|
| `webPageId` | integer | ID of the web page. |
| `webPageGuid` | guid | GUID of the web page. |
| `name` | string | The page's code name. |
| `contentType` | string | Code name of the page's content type, e.g. `DancingGoat.ArticlePage`. |
| `treePath` | string | The page's position in the content tree, e.g. `/Articles/Coffee_Beverages_Explained`. Not the URL. |
| `parentWebPageId` | integer | ID of the parent page, or null for a page at the root of the channel. |
| `languageName` | string | The language that was asked for (the page isn't checked to have a variant in it). |
| `channelName` | string | Code name of the website channel the page belongs to, e.g. `DancingGoatPages`. |
<!-- relay:end result:get-page-info -->

### Behavior { #get-page-info-behavior }

- **Tree path, not URL.** `treePath` is the page's position in the content tree (for example `/Articles/Coffee_Beverages_Explained`). For URLs use [`query-web-page-items`](#query-web-page-items) with `includeUrls`.
- **Parent.** `parentWebPageId` is `null` for a page at the root of the channel.
- **Language.** `languageName` in the answer is the language you asked for. The lookup doesn't check that the page has a variant in it.
- **Not found.** A page that doesn't exist fails with "Web page N was not found."

<!-- relay:begin examples:get-page-info -->
### Examples { #get-page-info-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Look up a page { #get-page-info-example-article }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-page-info",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetPageInfoCommand
    {
        WebPageId = 68,
    });
    var data = result.GetData<WebPageInfo>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "webPageId": 68,
    "webPageGuid": "5b308d07-cbb6-4ccb-8d85-6068cc689e17",
    "name": "ColdBrewAtHome-aqfgqi7s",
    "contentType": "DancingGoat.ArticlePage",
    "treePath": "/Articles/Cold_brew_at_home",
    "parentWebPageId": 1,
    "languageName": "en",
    "channelName": "DancingGoatPages"
  }
}
```
<!-- relay:end examples:get-page-info -->

<!-- relay:begin errors:get-page-info -->
### Errors { #get-page-info-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A page that does not exist"

    Parameters:

    ```json
    {
      "webPageId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Web page 99999 was not found.",
      "data": null
    }
    ```
<!-- relay:end errors:get-page-info -->

### See also { #get-page-info-see-also }

- [`get-page`](#get-page) for the field values as well
- [`query-web-page-items`](#query-web-page-items) to find pages
- [`move-web-page`](#move-web-page) and [`create-web-page`](#create-web-page) take IDs from here

---

## get-page

<!-- relay:begin header:get-page -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-page` | [Read](../../concepts/access-tiers.md) | [`WebPageData`](../../reference/types.md#webpagedata) | `GetPageCommand` |
<!-- relay:end header:get-page -->

Reads a page's system fields, the same ones [`get-page-info`](#get-page-info) returns, together with the field values of its latest version. The latest version includes drafts, so you see what editors see in the admin UI, not what the live site shows.

<!-- relay:begin parameters:get-page -->
### Parameters { #get-page-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:get-page -->

<!-- relay:begin result:get-page -->
### Result { #get-page-result }

A successful run returns a [WebPageData](../../reference/types.md#webpagedata) in `data`:

| Field | Type | Description |
|---|---|---|
| `webPageId` | integer | ID of the web page. |
| `webPageGuid` | guid | GUID of the web page. |
| `name` | string | The page's code name. |
| `contentType` | string | Code name of the page's content type, e.g. `DancingGoat.ArticlePage`. |
| `treePath` | string | The page's position in the content tree, e.g. `/Articles/Coffee_Beverages_Explained`. Not the URL. |
| `parentWebPageId` | integer | ID of the parent page, or null for a page at the root of the channel. |
| `languageName` | string | The language that was asked for (the page isn't checked to have a variant in it). |
| `channelName` | string | Code name of the website channel the page belongs to, e.g. `DancingGoatPages`. |
| `fields` | { name: any } | Field values of the page's latest version, by field name, as JSON (text, numbers, dates, ...). Asset, tag and reference fields come as the JSON text Kentico stores in them. Only the content type's own fields are listed: fields that come from reusable field schemas are not included. |
<!-- relay:end result:get-page -->

### Behavior { #get-page-behavior }

- **Field values.** `fields` is keyed by the content type's own field names. Text is a string, numbers and booleans are JSON values, dates are ISO strings without a time zone (`"2026-10-01T00:00:00"`), and a value that isn't set is `null`. Reference, tag and asset fields come as the JSON text Kentico stores in them: a string that contains JSON, for example `"[{\"Identifier\":\"b8117d96-...\"}]"`. Fields that come from reusable field schemas aren't included.
- **Published version.** To read what is live, use [`query-web-page-items`](#query-web-page-items) with `publishedOnly`.
- **Language.** `languageName` defaults to [`DefaultLanguageName`](../../configuration.md#defaultlanguagename), and the answer reports the language you asked for. A page that has no variant in it comes back with the values of the language Kentico falls back to.

<!-- relay:begin examples:get-page -->
### Examples { #get-page-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Read a page with its field values { #get-page-example-article }

The page is still an `InitialDraft`, and `get-page` returns its latest version, so the fields are the draft values.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-page",
      "parameters": {
        "webPageId": 68,
        "languageName": "en"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetPageCommand
    {
        WebPageId = 68,
        LanguageName = "en",
    });
    var data = result.GetData<WebPageData>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "fields": {
      "ArticleTitle": "Cold brew at home",
      "ArticlePagePublishDate": "2026-10-01T00:00:00",
      "ArticlePageTeaser": null,
      "ArticlePageSummary": "Steep coarse grounds in cold water overnight for a smooth, low-acid coffee.",
      "ArticlePageText": "<p>Mix 100 g of coarse grounds with 1 litre of cold water and leave it for 12 hours.</p>",
      "ArticleRelatedPages": null
    },
    "webPageId": 68,
    "webPageGuid": "5b308d07-cbb6-4ccb-8d85-6068cc689e17",
    "name": "ColdBrewAtHome-aqfgqi7s",
    "contentType": "DancingGoat.ArticlePage",
    "treePath": "/Articles/Cold_brew_at_home",
    "parentWebPageId": 1,
    "languageName": "en",
    "channelName": "DancingGoatPages"
  }
}
```
<!-- relay:end examples:get-page -->

<!-- relay:begin errors:get-page -->
<!-- relay:end errors:get-page -->

### See also { #get-page-see-also }

- [`get-page-info`](#get-page-info) when you don't need the fields
- [`update-web-page`](#update-web-page) to change them
- [`query-web-page-items`](#query-web-page-items) to read many pages at once

---

## query-web-page-items

<!-- relay:begin header:query-web-page-items -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `query-web-page-items` | [Read](../../concepts/access-tiers.md) | [`QueryItemsResult`](../../reference/types.md#queryitemsresult) | `QueryWebPageItemsCommand` |
<!-- relay:end header:query-web-page-items -->

Lists the pages of one or more content types in a channel and returns only the columns you name. It is the command for finding pages: by type, by the value of a field, by publication status, or together with their URLs.

<!-- relay:begin parameters:query-web-page-items -->
### Parameters { #query-web-page-items-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentTypeNames` | string[] | Yes | Code names of the content types to include, e.g. `DancingGoat.ArticlePage`. At least one. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `columns` | string[] | Yes | Columns to fetch and return. At least one. |
| `whereEquals` | { name: any } | No | Optional column-equality filters. All conditions are ANDed together. Each column must also be listed in `columns`. |
| `publishedOnly` | boolean | No | When true, returns the published version of each item, and leaves out items that have none. Defaults to false, which returns each item's latest version, drafts included. |
| `websiteChannelName` | string | No | Code name of the website channel to query. Defaults to [`RelayKenticoOptions.DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname) when null; the command fails if neither is set. |
| `includeUrls` | boolean | No | When true, each item also gets `WebPageRelativeUrl` and `WebPageAbsoluteUrl` (empty strings for pages with no URL path). Defaults to false. |
| `includeStatus` | boolean | No | When true, each item also gets `WebPageStatus` -- the status of its latest version: InitialDraft, Draft, Published or Unpublished -- and `WebPageIsPublished`: whether a published version exists. That is true for Published and for Draft, which is a change on top of a published version (a page that was never published is an InitialDraft), so a live page with unpublished changes has status Draft and is still published. The query still returns every page, drafts included. Defaults to false. |
<!-- relay:end parameters:query-web-page-items -->

<!-- relay:begin result:query-web-page-items -->
### Result { #query-web-page-items-result }

A successful run returns a [QueryItemsResult](../../reference/types.md#queryitemsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `items` | { name: any }[] | One entry per item, mapping each requested column to its value. |
<!-- relay:end result:query-web-page-items -->

### Behavior { #query-web-page-items-behavior }

- **Columns.** Name the columns you want, at least one: the content type's field names, plus system columns such as `WebPageItemID`, `WebPageItemGUID` and `WebPageUrlPath`. Each item in the answer is an object with just those columns. Reference and tag fields come back as JSON values (arrays of objects), not as strings.
- **Filtering.** `whereEquals` keeps the pages whose columns equal the values given; the conditions are combined with AND. A column used in `whereEquals` must also be listed in `columns`, otherwise the query fails with "Invalid column name". Values are strings, numbers or booleans.
- **Which version.** By default each page is its latest version, drafts included. With `publishedOnly: true` it is the published version of each page, field values included, and pages that have no published version are left out.
- **Several types.** `contentTypeNames` can list more than one content type; the answer is the union of their pages.
- **`includeStatus`.** Adds `WebPageStatus`, the status of the page's latest version (`InitialDraft`, `Draft`, `Published` or `Unpublished`), and `WebPageIsPublished`, which says whether a published version exists. That is `true` for `Published` and for `Draft` (a draft is a change on top of a published version) and `false` for `InitialDraft` and `Unpublished`. So a live page with unpublished changes has the status `Draft` and is still published. Both come from the same query, so they cost nothing extra.
- **`includeUrls`.** Adds `WebPageRelativeUrl` (`~/articles/cold-brew-guide`) and `WebPageAbsoluteUrl` (with the channel's domain), formatted the way the site serves them, which means lower case, so they can differ in case from `WebPageUrlPath`. Both are empty strings for a page with no URL path. They are the latest URLs: a published page with an unpublished URL change shows its new URL, unless `publishedOnly` is set. A page served through language fallback gets its fallback language's URL.
- **Language.** Pages come from `languageName`, with Kentico's language fallback. The status and URL describe the language variant a row comes from.
- **No paging.** The answer holds every matching page, so narrow the query with `whereEquals` when a type has many pages.
- **Channel.** `websiteChannelName` is the channel's code name; it defaults to [`DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname), and the command fails if neither is set.

<!-- relay:begin examples:query-web-page-items -->
### Examples { #query-web-page-items-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### List the article pages and their titles { #query-web-page-items-example-articles }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-web-page-items",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "contentTypeNames": [
          "DancingGoat.ArticlePage"
        ],
        "columns": [
          "WebPageItemID",
          "ArticleTitle"
        ]
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryWebPageItemsCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ContentTypeNames = ["DancingGoat.ArticlePage"],
        Columns = ["WebPageItemID", "ArticleTitle"],
    });
    var data = result.GetData<QueryItemsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "items": [
      {
        "WebPageItemID": 9,
        "ArticleTitle": "Coffee Beverages Explained"
      },
      {
        "WebPageItemID": 10,
        "ArticleTitle": "Coffee processing techniques"
      },
      {
        "WebPageItemID": 11,
        "ArticleTitle": "Donate with us"
      },
      {
        "WebPageItemID": 12,
        "ArticleTitle": "On Roasts"
      },
      {
        "WebPageItemID": 13,
        "ArticleTitle": "Origins of Arabica Bourbon"
      },
      {
        "WebPageItemID": 14,
        "ArticleTitle": "The Resilient Robusta: A Coffee with Character"
      },
      {
        "WebPageItemID": 15,
        "ArticleTitle": "Which brewing fits you?"
      },
      {
        "WebPageItemID": 68,
        "ArticleTitle": "Cold brew at home: a beginner's guide"
      },
      {
        "WebPageItemID": 69,
        "ArticleTitle": "Brewing methods compared"
      }
    ]
  }
}
```

#### Add each page's URL and publication status { #query-web-page-items-example-urls-and-status }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-web-page-items",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "contentTypeNames": [
          "DancingGoat.ArticlePage"
        ],
        "columns": [
          "WebPageItemID",
          "ArticleTitle"
        ],
        "whereEquals": {
          "WebPageItemID": 68
        },
        "includeUrls": true,
        "includeStatus": true
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryWebPageItemsCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ContentTypeNames = ["DancingGoat.ArticlePage"],
        Columns = ["WebPageItemID", "ArticleTitle"],
        WhereEquals = new()
        {
            ["WebPageItemID"] = JsonSerializer.SerializeToElement(68),
        },
        IncludeUrls = true,
        IncludeStatus = true,
    });
    var data = result.GetData<QueryItemsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "items": [
      {
        "WebPageItemID": 68,
        "ArticleTitle": "Cold brew at home: a beginner's guide",
        "WebPageStatus": "Published",
        "WebPageIsPublished": true,
        "WebPageRelativeUrl": "~/articles/cold-brew-guide",
        "WebPageAbsoluteUrl": "https://localhost/articles/cold-brew-guide"
      }
    ]
  }
}
```

#### Only the published version of each page { #query-web-page-items-example-published-only }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-web-page-items",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "contentTypeNames": [
          "DancingGoat.ArticlePage"
        ],
        "columns": [
          "WebPageItemID",
          "ArticleTitle"
        ],
        "whereEquals": {
          "WebPageItemID": 68
        },
        "publishedOnly": true
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryWebPageItemsCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ContentTypeNames = ["DancingGoat.ArticlePage"],
        Columns = ["WebPageItemID", "ArticleTitle"],
        WhereEquals = new()
        {
            ["WebPageItemID"] = JsonSerializer.SerializeToElement(68),
        },
        PublishedOnly = true,
    });
    var data = result.GetData<QueryItemsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "items": [
      {
        "WebPageItemID": 68,
        "ArticleTitle": "Cold brew at home: a beginner's guide"
      }
    ]
  }
}
```
<!-- relay:end examples:query-web-page-items -->

<!-- relay:begin errors:query-web-page-items -->
### Errors { #query-web-page-items-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No columns named"

    Parameters:

    ```json
    {
      "websiteChannelName": "DancingGoatPages",
      "contentTypeNames": [
        "DancingGoat.ArticlePage"
      ]
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Columns must list at least one column.",
      "data": null
    }
    ```
<!-- relay:end errors:query-web-page-items -->

### See also { #query-web-page-items-see-also }

- [`get-page-info`](#get-page-info) and [`get-page`](#get-page) for one page
- [`query-reusable-items`](../content-items/index.md#query-reusable-items) for reusable content
- [`search-content`](../content-items/index.md#search-content) to search text
- [Query content](../../guides/querying.md)
