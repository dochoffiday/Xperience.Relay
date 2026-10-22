---
toc_depth: 2
---

# Content items

Reusable content items live in the content hub, in a workspace and optionally in a folder. Every content item command takes the item's ID (`contentItemId`): [`create-content-item`](#create-content-item) returns it, and [`query-reusable-items`](#query-reusable-items) or [`search-content`](#search-content) find it for items that already exist.

An item has one variant per language, and changes go through drafts like pages do: see [Drafts and publishing](../../concepts/drafts-and-publishing.md). The commands for pages and for items are separate, and an ID of the wrong kind is refused with a pointer to the right command.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`create-content-item`](#create-content-item) | Write | Creates a new reusable content item and publishes it, optionally uploading one or more binary assets into asset fields. |
| [`update-content-item`](#update-content-item) | Write | Updates one or more fields on an existing reusable content item. |
| [`move-content-item`](#move-content-item) | Write | Moves one or more reusable content items into a content hub folder. |
| [`publish-content-item`](#publish-content-item) | Write | Publishes a reusable content item language variant: a draft is published, and an unpublished item gets a draft made first. |
| [`unpublish-content-item`](#unpublish-content-item) | Write | Unpublishes a reusable content item language variant. |
| [`delete-content-item`](#delete-content-item) | Write | Deletes a language variant of a reusable content item. |
| [`get-content-info`](#get-content-info) | Read | Fetches a reusable content item's system fields (content type, language, workspace, ...) without its content-type-specific field data. |
| [`get-content`](#get-content) | Read | Fetches a reusable content item's system fields plus the field values of its latest version (drafts included). |
| [`get-content-item-usage`](#get-content-item-usage) | Read | Finds the content items that directly reference a content item's language variant (the "used by" list in the admin UI). |
| [`get-content-hub-folder`](#get-content-hub-folder) | Write | Looks up a content hub folder by ID, codename, or path. |
| [`query-reusable-items`](#query-reusable-items) | Read | Queries reusable content items of one or more content types, returning only the requested columns. |
| [`search-content`](#search-content) | Read | Searches the published items of one content type for text, case-insensitively, across every text, long text and rich text field. |
<!-- relay:end commands -->

---

## create-content-item

<!-- relay:begin header:create-content-item -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-content-item` | [Write](../../concepts/access-tiers.md) | [`CreateContentItemResult`](../../reference/types.md#createcontentitemresult) | `CreateContentItemCommand` |
<!-- relay:end header:create-content-item -->

Creates a reusable content item and publishes it. It can put the item in a content hub folder and upload files into its asset fields in the same call.

<!-- relay:begin parameters:create-content-item -->
### Parameters { #create-content-item-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentTypeName` | string | Yes | Code name of a reusable content type, e.g. `Podcast.Episode`. |
| `displayName` | string | Yes | Display name of the new item. Longer names are cut to 100 characters. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `workspaceName` | string | No | Defaults to [`RelayKenticoOptions.DefaultWorkspaceName`](../../configuration.md#defaultworkspacename) when null. |
| `contentFolderId` | integer | No | Content hub folder to place the item in after creation. Use the [`get-content-hub-folder`](#get-content-hub-folder) command first to resolve or create the target folder and get its ID. |
| `fields` | { name: any } | No | Plain field values by field name (text, numbers, booleans, dates, ...). They are checked against the content type before anything is created. |
| `linkedItemFields` | { name: guid[] } | No | Linked-items fields. Maps field name to a list of ContentItemGUIDs. The field must be a content item reference field. |
| `tagFields` | { name: guid[] } | No | Tag fields. Maps field name to a list of tag GUIDs. The field must be a tag (taxonomy) field. |
| `assets` | [RelayAsset](../../reference/types.md#relayasset)[] | No | Optional binary assets to upload into asset fields. Each entry maps to one field. |
<!-- relay:end parameters:create-content-item -->

<!-- relay:begin result:create-content-item -->
### Result { #create-content-item-result }

A successful run returns a [CreateContentItemResult](../../reference/types.md#createcontentitemresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `contentItemGuid` | guid | GUID of the new item. |
| `contentItemId` | integer | ID of the new item. |
<!-- relay:end result:create-content-item -->

### Behavior { #create-content-item-behavior }

- **Published at once.** There is no draft step: the item is created and published, and the answer says so ("Created and published ..."). If Kentico creates the item but declines to publish it, the result is a failure that names the new item's ID.
- **Reusable types only.** `contentTypeName` must be a reusable content type; page types go through [`create-web-page`](../web-pages/index.md#create-web-page). An unknown type fails with "Content type '...' was not found."
- **Workspace.** `workspaceName` is the workspace's code name (for example `DancingGoat.DancingGoatEvents`). When it is left out Relay uses [`DefaultWorkspaceName`](../../configuration.md#defaultworkspacename), and the command fails if that isn't set either.
- **Folder.** Give `contentFolderId` to move the new item into a content hub folder once it is created. [`get-content-hub-folder`](#get-content-hub-folder) finds or creates the folder and returns its ID.
- **Name.** `displayName` is cut to 100 characters. Kentico makes the item's code name from it.
- **Fields.** They are checked the way [`create-web-page`](../web-pages/index.md#create-web-page) checks them: unknown fields and values that don't fit are refused, all problems at once, and nothing is created. `linkedItemFields` takes content item GUIDs, `tagFields` takes tag GUIDs and `assets` takes Base64 files. See [Upload files](../../guides/uploading-assets.md).
- **The result.** `contentItemId` is the ID the other content item commands take. `contentItemGuid` is what reference fields of other items use.

<!-- relay:begin examples:create-content-item -->
### Examples { #create-content-item-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Create an event in a content hub folder { #create-content-item-example-event }

The item is published as soon as it is created. `EventCafe` is a content item reference field, so it takes the GUID of the Boston cafe item.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-content-item",
      "parameters": {
        "contentTypeName": "DancingGoat.Event",
        "displayName": "Cold brew workshop",
        "workspaceName": "DancingGoat.DancingGoatEvents",
        "contentFolderId": 5,
        "fields": {
          "EventTitle": "Cold brew workshop",
          "EventPromoText": "Learn to make cold brew at home.",
          "EventDate": "2026-11-14T10:00:00"
        },
        "linkedItemFields": {
          "EventCafe": [
            "4c84719d-689e-460e-8cc1-5495c367b249"
          ]
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateContentItemCommand
    {
        ContentTypeName = "DancingGoat.Event",
        DisplayName = "Cold brew workshop",
        WorkspaceName = "DancingGoat.DancingGoatEvents",
        ContentFolderId = 5,
        Fields = new()
        {
            ["EventTitle"] = JsonSerializer.SerializeToElement("Cold brew workshop"),
            ["EventPromoText"] = JsonSerializer.SerializeToElement("Learn to make cold brew at home."),
            ["EventDate"] = JsonSerializer.SerializeToElement("2026-11-14T10:00:00"),
        },
        LinkedItemFields = new()
        {
            ["EventCafe"] = [Guid.Parse("4c84719d-689e-460e-8cc1-5495c367b249")],
        },
    });
    var data = result.GetData<CreateContentItemResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created and published 'Cold brew workshop' (ID=184).",
  "error": null,
  "data": {
    "contentItemGuid": "a244387c-f5fc-4fe2-861c-d33832e7b90d",
    "contentItemId": 184
  }
}
```

#### Create an item with a tag field { #create-content-item-example-with-tags }

`CafePromotion` is a tag field, so it takes tag GUIDs (see [`get-tags`](../taxonomies/index.md#get-tags)).

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-content-item",
      "parameters": {
        "contentTypeName": "DancingGoat.Cafe",
        "displayName": "Seattle Coffee Place",
        "workspaceName": "DancingGoat.DancingGoatLtd",
        "fields": {
          "CafeName": "Seattle Coffee Place",
          "CafeStreet": "1 Pike Place",
          "CafeCity": "Seattle",
          "CafeCountry": "USA"
        },
        "tagFields": {
          "CafePromotion": [
            "9966ec69-20f5-40f3-9833-9122ea53e416"
          ]
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateContentItemCommand
    {
        ContentTypeName = "DancingGoat.Cafe",
        DisplayName = "Seattle Coffee Place",
        WorkspaceName = "DancingGoat.DancingGoatLtd",
        Fields = new()
        {
            ["CafeName"] = JsonSerializer.SerializeToElement("Seattle Coffee Place"),
            ["CafeStreet"] = JsonSerializer.SerializeToElement("1 Pike Place"),
            ["CafeCity"] = JsonSerializer.SerializeToElement("Seattle"),
            ["CafeCountry"] = JsonSerializer.SerializeToElement("USA"),
        },
        TagFields = new()
        {
            ["CafePromotion"] = [Guid.Parse("9966ec69-20f5-40f3-9833-9122ea53e416")],
        },
    });
    var data = result.GetData<CreateContentItemResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created and published 'Seattle Coffee Place' (ID=185).",
  "error": null,
  "data": {
    "contentItemGuid": "4d4e1a87-0754-498a-9ddf-5e86f7f9d4a7",
    "contentItemId": 185
  }
}
```

#### Create an item with an uploaded file { #create-content-item-example-with-asset }

The file travels as Base64 text inside the JSON. This one is a 1 x 1 pixel PNG.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-content-item",
      "parameters": {
        "contentTypeName": "DancingGoat.Image",
        "displayName": "Cold brew diagram",
        "workspaceName": "DancingGoat.DancingGoatLtd",
        "fields": {
          "ImageShortDescription": "How cold brew is made"
        },
        "assets": [
          {
            "fieldName": "ImageFile",
            "fileName": "cold-brew.png",
            "base64": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
          }
        ]
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateContentItemCommand
    {
        ContentTypeName = "DancingGoat.Image",
        DisplayName = "Cold brew diagram",
        WorkspaceName = "DancingGoat.DancingGoatLtd",
        Fields = new()
        {
            ["ImageShortDescription"] = JsonSerializer.SerializeToElement("How cold brew is made"),
        },
        Assets = [
            new RelayAsset
            {
                FieldName = "ImageFile",
                FileName = "cold-brew.png",
                Base64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            },
        ],
    });
    var data = result.GetData<CreateContentItemResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created and published 'Cold brew diagram' (ID=186).",
  "error": null,
  "data": {
    "contentItemGuid": "86c59020-2715-4c87-8bc3-5b29ecabaf62",
    "contentItemId": 186
  }
}
```
<!-- relay:end examples:create-content-item -->

<!-- relay:begin errors:create-content-item -->
### Errors { #create-content-item-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A content type that does not exist"

    Parameters:

    ```json
    {
      "contentTypeName": "DancingGoat.Podcast",
      "displayName": "Episode 1",
      "workspaceName": "DancingGoat.DancingGoatLtd"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Content type 'DancingGoat.Podcast' was not found.",
      "data": null
    }
    ```

???+ failure "Fields the content type does not have, or values that do not fit"

    Parameters:

    ```json
    {
      "contentTypeName": "DancingGoat.Event",
      "displayName": "Cold brew workshop",
      "workspaceName": "DancingGoat.DancingGoatEvents",
      "fields": {
        "EventTitle": "Cold brew workshop",
        "EventLocation": "Boston",
        "EventDate": "next Friday"
      },
      "linkedItemFields": {
        "EventTitle": [
          "4c84719d-689e-460e-8cc1-5495c367b249"
        ]
      }
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Field 'EventLocation' was not found on content type 'DancingGoat.Event'. Field 'EventDate' expects a date, but got 'next Friday'. Field 'EventTitle' is not a content item reference field (it is text), so it can't be set from LinkedItemFields.",
      "data": null
    }
    ```

???+ failure "No workspace named, and no default configured"

    Parameters:

    ```json
    {
      "contentTypeName": "DancingGoat.Event",
      "displayName": "Cold brew workshop"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "WorkspaceName is required. Set it on the command or configure RelayKenticoOptions.DefaultWorkspaceName.",
      "data": null
    }
    ```
<!-- relay:end errors:create-content-item -->

### See also { #create-content-item-see-also }

- [`update-content-item`](#update-content-item) to change it later
- [`get-content-hub-folder`](#get-content-hub-folder) to find a folder
- [Upload files](../../guides/uploading-assets.md)

---

## update-content-item

<!-- relay:begin header:update-content-item -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateContentItemCommand` |
<!-- relay:end header:update-content-item -->

Changes fields of a reusable content item in one language, through a draft, and leaves the item in the state it found it in. It is the reusable-item counterpart of [`update-web-page`](../web-pages/index.md#update-web-page).

<!-- relay:begin parameters:update-content-item -->
### Parameters { #update-content-item-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `fields` | { name: any } | No | Plain field values by field name (text, numbers, booleans, dates, ...). |
| `linkedItemFields` | { name: guid[] } | No | Linked-items fields. Maps field name to a list of ContentItemGUIDs. The field must be a content item reference field. An empty list clears the field. |
| `tagFields` | { name: guid[] } | No | Tag fields. Maps field name to a list of tag GUIDs. The field must be a tag (taxonomy) field. An empty list clears the field. |
| `assets` | [RelayAsset](../../reference/types.md#relayasset)[] | No | Optional binary assets to upload into asset fields. Each entry maps to one field. |
<!-- relay:end parameters:update-content-item -->

<!-- relay:begin result:update-content-item -->
### Result { #update-content-item-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:update-content-item -->

### Behavior { #update-content-item-behavior }

- **State is kept.** A published item is published again with the change, and a draft or unpublished item stays that way. The table in [`update-web-page`](../web-pages/index.md#update-web-page) applies as it is, and so do the messages ("Saved as a draft; it is not published.").
- **Failures leave nothing behind.** If a step fails, a draft made for the edit is discarded again and Kentico's cached copy is dropped, exactly as for pages.
- **Checks.** Fields are checked against the content type before anything changes, all problems at once.
- **Clearing a field.** An empty list in `linkedItemFields` or `tagFields` clears that field.
- **Reusable items only.** The content item of a web page is refused with "Content item N is not a reusable content item. Use update-web-page for web pages." An item with no variant in `languageName` fails with "was not found in that language".
- **Something to change.** At least one of `fields`, `linkedItemFields`, `tagFields` and `assets` must be given.

<!-- relay:begin examples:update-content-item -->
### Examples { #update-content-item-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Update an item that is published { #update-content-item-example-published }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-content-item",
      "parameters": {
        "contentItemId": 184,
        "fields": {
          "EventPromoText": "Learn to make cold brew at home - bring your own jar.",
          "EventDate": "2026-11-21T10:00:00"
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateContentItemCommand
    {
        ContentItemId = 184,
        Fields = new()
        {
            ["EventPromoText"] = JsonSerializer.SerializeToElement("Learn to make cold brew at home - bring your own jar."),
            ["EventDate"] = JsonSerializer.SerializeToElement("2026-11-21T10:00:00"),
        },
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated 2 field(s) on content item 184.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:update-content-item -->

<!-- relay:begin errors:update-content-item -->
### Errors { #update-content-item-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "Nothing to update"

    Parameters:

    ```json
    {
      "contentItemId": 184
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "No fields provided — nothing to update.",
      "data": null
    }
    ```
<!-- relay:end errors:update-content-item -->

### See also { #update-content-item-see-also }

- [`update-web-page`](../web-pages/index.md#update-web-page)
- [`publish-content-item`](#publish-content-item) to put a draft live
- [`get-content`](#get-content) to read the fields back
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)

---

## move-content-item

<!-- relay:begin header:move-content-item -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `move-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `MoveContentItemCommand` |
<!-- relay:end header:move-content-item -->

Moves one or more reusable content items into a content hub folder.

<!-- relay:begin parameters:move-content-item -->
### Parameters { #move-content-item-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemIds` | integer[] | Yes | IDs of the content items to move. At least one. |
| `contentFolderId` | integer | Yes | ID of the destination content hub folder. |
<!-- relay:end parameters:move-content-item -->

<!-- relay:begin result:move-content-item -->
### Result { #move-content-item-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:move-content-item -->

### Behavior { #move-content-item-behavior }

- **The folder.** `contentFolderId` is the destination folder's ID; [`get-content-hub-folder`](#get-content-hub-folder) finds or creates it. A folder that doesn't exist fails with Kentico's "The content folder with ID ... does not exist."
- **The items.** Every ID has to be a reusable content item. An ID that doesn't exist, or that belongs to a web page, fails with a message that says so, and nothing is moved.
- **Many at once.** All of `contentItemIds` are moved in one call.

<!-- relay:begin examples:move-content-item -->
### Examples { #move-content-item-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Move an item to another folder { #move-content-item-example-folder }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "move-content-item",
      "parameters": {
        "contentItemIds": [
          184
        ],
        "contentFolderId": 6
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new MoveContentItemCommand
    {
        ContentItemIds = [184],
        ContentFolderId = 6,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Moved 1 content item(s) to folder 6.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:move-content-item -->

<!-- relay:begin errors:move-content-item -->
### Errors { #move-content-item-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No items named"

    Parameters:

    ```json
    {
      "contentItemIds": [],
      "contentFolderId": 6
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "ContentItemIds must not be empty.",
      "data": null
    }
    ```

???+ failure "An item that does not exist"

    Parameters:

    ```json
    {
      "contentItemIds": [
        99999
      ],
      "contentFolderId": 6
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Content item 99999 was not found.",
      "data": null
    }
    ```

???+ failure "The content item behind a web page"

    Content item 132 belongs to the page "Coffee Beverages Explained". Pages are moved with `move-web-page`.

    Parameters:

    ```json
    {
      "contentItemIds": [
        132
      ],
      "contentFolderId": 6
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Content item 132 is not a reusable content item. Use move-web-page for web pages.",
      "data": null
    }
    ```

???+ failure "A folder that does not exist"

    Parameters:

    ```json
    {
      "contentItemIds": [
        184
      ],
      "contentFolderId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Unhandled exception executing verb 'move-content-item': [ContentFolderNotFoundException] The content folder with ID 99999 does not exist.",
      "data": null
    }
    ```
<!-- relay:end errors:move-content-item -->

### See also { #move-content-item-see-also }

- [`get-content-hub-folder`](#get-content-hub-folder)
- [`create-content-item`](#create-content-item), which can place an item in a folder as it creates it

---

## publish-content-item

<!-- relay:begin header:publish-content-item -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `publish-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `PublishContentItemCommand` |
<!-- relay:end header:publish-content-item -->

Publishes a reusable content item in one language.

<!-- relay:begin parameters:publish-content-item -->
### Parameters { #publish-content-item-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:publish-content-item -->

<!-- relay:begin result:publish-content-item -->
### Result { #publish-content-item-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:publish-content-item -->

### Behavior { #publish-content-item-behavior }

What happens depends on the item's latest version:

| Latest version | What Relay does |
|---|---|
| A draft (including a draft over a published version) | Publishes the draft |
| Unpublished | Makes a draft first, then publishes it, so an unpublished item can be published again |
| Published | Nothing; the answer says the item is already published |

- **Safe to repeat.** Asking for a state the item is already in succeeds without changing anything.
- **Never a false answer.** An item, or a variant in that language, that doesn't exist is a failure, and so is Kentico declining to publish.
- **Reusable items only.** The content item of a web page is refused with a pointer to the page commands; use [`publish-web-page`](../web-pages/index.md#publish-web-page) for pages.

<!-- relay:begin examples:publish-content-item -->
### Examples { #publish-content-item-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Publish an item that was unpublished { #publish-content-item-example-unpublished }

An unpublished item gets a draft first, then that draft is published.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-content-item",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishContentItemCommand
    {
        ContentItemId = 184,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Published content item 184 (en).",
  "error": null,
  "data": null
}
```

#### Publish an item that is already published { #publish-content-item-example-already-published }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-content-item",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishContentItemCommand
    {
        ContentItemId = 184,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Content item 184 (en) is already published.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:publish-content-item -->

<!-- relay:begin errors:publish-content-item -->
### Errors { #publish-content-item-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "An item that does not exist"

    Parameters:

    ```json
    {
      "contentItemId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Content item 99999 was not found.",
      "data": null
    }
    ```
<!-- relay:end errors:publish-content-item -->

### See also { #publish-content-item-see-also }

- [`unpublish-content-item`](#unpublish-content-item)
- [`update-content-item`](#update-content-item)
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)

---

## unpublish-content-item

<!-- relay:begin header:unpublish-content-item -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `unpublish-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UnpublishContentItemCommand` |
<!-- relay:end header:unpublish-content-item -->

Takes a published reusable content item in one language offline.

<!-- relay:begin parameters:unpublish-content-item -->
### Parameters { #unpublish-content-item-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:unpublish-content-item -->

<!-- relay:begin result:unpublish-content-item -->
### Result { #unpublish-content-item-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:unpublish-content-item -->

### Behavior { #unpublish-content-item-behavior }

What happens depends on the item's latest version:

| Latest version | What Relay does |
|---|---|
| Published | Unpublishes it |
| Unpublished | Nothing; the answer says it is already unpublished |
| A draft that was never published | Nothing; the answer says the item isn't published and only has a draft |
| Published, with a newer draft | Refuses: Kentico won't unpublish an item while it has a draft with unpublished changes. Publish or discard the draft first |

- **The content stays.** An unpublished item isn't deleted; [`publish-content-item`](#publish-content-item) puts it back online.
- **Reusable items only.** Use [`unpublish-web-page`](../web-pages/index.md#unpublish-web-page) for pages.

<!-- relay:begin examples:unpublish-content-item -->
### Examples { #unpublish-content-item-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Take a published item offline { #unpublish-content-item-example-published }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "unpublish-content-item",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UnpublishContentItemCommand
    {
        ContentItemId = 184,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Unpublished content item 184 (en).",
  "error": null,
  "data": null
}
```

#### Unpublish an item that is already unpublished { #unpublish-content-item-example-already-unpublished }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "unpublish-content-item",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UnpublishContentItemCommand
    {
        ContentItemId = 184,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Content item 184 (en) is already unpublished.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:unpublish-content-item -->

<!-- relay:begin errors:unpublish-content-item -->
<!-- relay:end errors:unpublish-content-item -->

### See also { #unpublish-content-item-see-also }

- [`publish-content-item`](#publish-content-item)
- [`delete-content-item`](#delete-content-item) to remove an item instead
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)

---

## delete-content-item

<!-- relay:begin header:delete-content-item -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteContentItemCommand` |
<!-- relay:end header:delete-content-item -->

Deletes a reusable content item in one language. The item goes to the recycle bin, where it can be restored from the admin UI.

<!-- relay:begin parameters:delete-content-item -->
### Parameters { #delete-content-item-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:delete-content-item -->

<!-- relay:begin result:delete-content-item -->
### Result { #delete-content-item-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:delete-content-item -->

### Behavior { #delete-content-item-behavior }

- **Recycle bin.** Relay has no gone-for-good deletion for content items.
- **The language.** The command deletes the language variant you name. When that is the item's only variant, the item itself is deleted.
- **References aren't checked.** Kentico deletes an item even when other items refer to it, and those references are left pointing at nothing. Run [`get-content-item-usage`](#get-content-item-usage) first to see what would be affected.
- **Missing items.** An item that doesn't exist, an item that is a web page's content item (use [`delete-web-page`](../web-pages/index.md#delete-web-page)), and an item with no variant in `languageName` all fail with a message that says so. Relay never reports a deletion that didn't happen.

<!-- relay:begin examples:delete-content-item -->
### Examples { #delete-content-item-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Delete an item { #delete-content-item-example-event }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-content-item",
      "parameters": {
        "contentItemId": 184,
        "languageName": "en"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteContentItemCommand
    {
        ContentItemId = 184,
        LanguageName = "en",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted content item 184 (en).",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:delete-content-item -->

<!-- relay:begin errors:delete-content-item -->
### Errors { #delete-content-item-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "An item that does not exist"

    Parameters:

    ```json
    {
      "contentItemId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Content item 99999 was not found.",
      "data": null
    }
    ```
<!-- relay:end errors:delete-content-item -->

### See also { #delete-content-item-see-also }

- [`get-content-item-usage`](#get-content-item-usage) to see what refers to an item
- [`unpublish-content-item`](#unpublish-content-item) to take an item offline instead
- [`delete-web-page`](../web-pages/index.md#delete-web-page)

---

## get-content-info

<!-- relay:begin header:get-content-info -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-content-info` | [Read](../../concepts/access-tiers.md) | [`ContentInfo`](../../reference/types.md#contentinfo) | `GetContentInfoCommand` |
<!-- relay:end header:get-content-info -->

Looks up a reusable content item's system fields: its code name, content type and workspace. It returns no field values, so it is the cheap way to check that an item exists.

<!-- relay:begin parameters:get-content-info -->
### Parameters { #get-content-info-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
<!-- relay:end parameters:get-content-info -->

<!-- relay:begin result:get-content-info -->
### Result { #get-content-info-result }

A successful run returns a [ContentInfo](../../reference/types.md#contentinfo) in `data`:

| Field | Type | Description |
|---|---|---|
| `contentItemId` | integer | ID of the content item. |
| `contentItemGuid` | guid | GUID of the content item. |
| `name` | string | The item's code name. |
| `contentType` | string | Code name of the item's content type, e.g. `DancingGoat.Image`. |
| `languageName` | string | Empty from [`get-content-info`](#get-content-info) (the item's system fields don't vary by language); the requested language from [`get-content`](#get-content). |
| `workspaceName` | string | The workspace's numeric ID, as text. Kentico's public API doesn't expose a workspace's name. |
| `contentFolderPath` | string | Reserved. Not filled in yet, so currently always null. |
<!-- relay:end result:get-content-info -->

### Behavior { #get-content-info-behavior }

- **No language.** `languageName` is empty in the answer, because the system fields don't vary by language. [`get-content`](#get-content) reports the language you asked for.
- **Workspace.** `workspaceName` holds the workspace's numeric ID as text (for example `"2"`), because Kentico's public API doesn't expose a workspace's name.
- **Folder.** `contentFolderPath` is not filled in yet and is always `null`.
- **Not found.** An item that doesn't exist fails with "Content item N was not found."

<!-- relay:begin examples:get-content-info -->
### Examples { #get-content-info-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Look up a content item { #get-content-info-example-event }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-info",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentInfoCommand
    {
        ContentItemId = 184,
    });
    var data = result.GetData<ContentInfo>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "contentItemId": 184,
    "contentItemGuid": "a244387c-f5fc-4fe2-861c-d33832e7b90d",
    "name": "ColdBrewWorkshop-23s43e2s",
    "contentType": "DancingGoat.Event",
    "languageName": "",
    "workspaceName": "2",
    "contentFolderPath": null
  }
}
```
<!-- relay:end examples:get-content-info -->

<!-- relay:begin errors:get-content-info -->
### Errors { #get-content-info-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "An item that does not exist"

    Parameters:

    ```json
    {
      "contentItemId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Content item 99999 was not found.",
      "data": null
    }
    ```
<!-- relay:end errors:get-content-info -->

### See also { #get-content-info-see-also }

- [`get-content`](#get-content) for the field values as well
- [`query-reusable-items`](#query-reusable-items) to find items

---

## get-content

<!-- relay:begin header:get-content -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-content` | [Read](../../concepts/access-tiers.md) | [`ContentData`](../../reference/types.md#contentdata) | `GetContentCommand` |
<!-- relay:end header:get-content -->

Reads a reusable content item's system fields, the same ones [`get-content-info`](#get-content-info) returns, together with the field values of its latest version. The latest version includes drafts, so you see what editors see in the admin UI, not what the live site shows.

<!-- relay:begin parameters:get-content -->
### Parameters { #get-content-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:get-content -->

<!-- relay:begin result:get-content -->
### Result { #get-content-result }

A successful run returns a [ContentData](../../reference/types.md#contentdata) in `data`:

| Field | Type | Description |
|---|---|---|
| `contentItemId` | integer | ID of the content item. |
| `contentItemGuid` | guid | GUID of the content item. |
| `name` | string | The item's code name. |
| `contentType` | string | Code name of the item's content type, e.g. `DancingGoat.Image`. |
| `languageName` | string | Empty from [`get-content-info`](#get-content-info) (the item's system fields don't vary by language); the requested language from [`get-content`](#get-content). |
| `workspaceName` | string | The workspace's numeric ID, as text. Kentico's public API doesn't expose a workspace's name. |
| `contentFolderPath` | string | Reserved. Not filled in yet, so currently always null. |
| `fields` | { name: any } | Field values of the item's latest version, by field name, as JSON (text, numbers, dates, ...). Asset, tag and reference fields come as the JSON text Kentico stores in them. Only the content type's own fields are listed: fields that come from reusable field schemas are not included. |
<!-- relay:end result:get-content -->

### Behavior { #get-content-behavior }

- **Field values.** `fields` is keyed by the content type's own field names. Text is a string, numbers and booleans are JSON values, dates are ISO strings without a time zone (`"2026-11-14T10:00:00"`), and a value that isn't set is `null`. Reference, tag and asset fields come as the JSON text Kentico stores in them: a string that contains JSON. Fields that come from reusable field schemas aren't included.
- **Published version.** To read what is live, use [`query-reusable-items`](#query-reusable-items) with `publishedOnly`.
- **Language.** `languageName` defaults to [`DefaultLanguageName`](../../configuration.md#defaultlanguagename), and the answer reports the language you asked for. An item that has no variant in it comes back with the values of the language Kentico falls back to.

<!-- relay:begin examples:get-content -->
### Examples { #get-content-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Read an item with its field values { #get-content-example-event }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentCommand
    {
        ContentItemId = 184,
    });
    var data = result.GetData<ContentData>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "fields": {
      "EventTitle": "Cold brew workshop",
      "EventHeroBannerImage": null,
      "EventPromoText": "Learn to make cold brew at home.",
      "EventDate": "2026-11-14T10:00:00",
      "EventCafe": "[{\"Identifier\":\"4c84719d-689e-460e-8cc1-5495c367b249\"}]"
    },
    "contentItemId": 184,
    "contentItemGuid": "a244387c-f5fc-4fe2-861c-d33832e7b90d",
    "name": "ColdBrewWorkshop-23s43e2s",
    "contentType": "DancingGoat.Event",
    "languageName": "en",
    "workspaceName": "2",
    "contentFolderPath": null
  }
}
```

#### Read an item with an asset field { #get-content-example-asset }

`ImageFile` comes back as the JSON text Kentico stores for the asset.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content",
      "parameters": {
        "contentItemId": 186
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentCommand
    {
        ContentItemId = 186,
    });
    var data = result.GetData<ContentData>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "fields": {
      "ImageFile": "{\"Original\":{\"Identifier\":\"b73333e9-7170-40e0-bd10-21f8052d8d1c\",\"Name\":\"cold-brew.png\",\"Extension\":\".png\",\"Size\":70,\"Width\":1,\"Height\":1,\"LastModified\":\"2026-10-20T17:52:04.4989026Z\"},\"OptimizationParameters\":{\"OutputExtension\":\"webp\",\"Quality\":70,\"WidthLimit\":null,\"HeightLimit\":null},\"Variants\":{},\"FocalPoint\":{\"X\":0,\"Y\":0},\"Identifier\":\"94464018-4b98-48a3-9622-d3de3eea2556\",\"Name\":\"cold-brew.webp\",\"Extension\":\".webp\",\"Size\":96,\"Width\":1,\"Height\":1,\"LastModified\":\"2026-10-20T17:52:04.6628406Z\"}",
      "ImageShortDescription": "How cold brew is made"
    },
    "contentItemId": 186,
    "contentItemGuid": "86c59020-2715-4c87-8bc3-5b29ecabaf62",
    "name": "ColdBrewDiagram-edivq6ya",
    "contentType": "DancingGoat.Image",
    "languageName": "en",
    "workspaceName": "3",
    "contentFolderPath": null
  }
}
```
<!-- relay:end examples:get-content -->

<!-- relay:begin errors:get-content -->
<!-- relay:end errors:get-content -->

### See also { #get-content-see-also }

- [`get-content-info`](#get-content-info) when you don't need the fields
- [`update-content-item`](#update-content-item) to change them
- [`query-reusable-items`](#query-reusable-items) to read many items at once

---

## get-content-item-usage

<!-- relay:begin header:get-content-item-usage -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-content-item-usage` | [Read](../../concepts/access-tiers.md) | [`GetContentItemUsageResult`](../../reference/types.md#getcontentitemusageresult) | `GetContentItemUsageCommand` |
<!-- relay:end header:get-content-item-usage -->

Lists the content items that directly refer to one language variant of a content item: the "used by" list of the admin UI.

<!-- relay:begin parameters:get-content-item-usage -->
### Parameters { #get-content-item-usage-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the content item (reusable item or page content item) to look up. Must be positive. |
| `languageName` | string | Yes | Language variant to look up, e.g. `en`. There is no default. |
<!-- relay:end parameters:get-content-item-usage -->

<!-- relay:begin result:get-content-item-usage -->
### Result { #get-content-item-usage-result }

A successful run returns a [GetContentItemUsageResult](../../reference/types.md#getcontentitemusageresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `items` | [ContentItemUsageEntry](../../reference/types.md#contentitemusageentry)[] | The content items that reference the item, one entry per language variant. |
<!-- relay:end result:get-content-item-usage -->

### Behavior { #get-content-item-usage-behavior }

- **Check before deleting.** Kentico lets you delete an item that other items refer to, and the references are left pointing at nothing. [`delete-content-item`](#delete-content-item) doesn't warn you; this command does the looking.
- **One entry per referencing variant.** An item can appear twice, once for each language variant that refers to it (the example shows an event in Spanish and in English). For a web page, `contentItemId` is the page's content item ID, not its web page ID.
- **Direct references only.** Items that refer to the referencing items aren't followed.
- **The language is required.** There is no default: `languageName` says which variant of the item to look at, and leaving it out fails with "LanguageName is required."

<!-- relay:begin examples:get-content-item-usage -->
### Examples { #get-content-item-usage-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Which items reference the Boston cafe? { #get-content-item-usage-example-cafe }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-item-usage",
      "parameters": {
        "contentItemId": 19,
        "languageName": "en"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentItemUsageCommand
    {
        ContentItemId = 19,
        LanguageName = "en",
    });
    var data = result.GetData<GetContentItemUsageResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found 4 usage(s) for content item 19 in language 'en'.",
  "error": null,
  "data": {
    "items": [
      {
        "contentItemId": 84,
        "languageName": "es",
        "displayName": "¡Nuestra primera cata ha llegado!",
        "latestVersionStatus": "Published",
        "createdWhen": "2023-09-07T07:23:55",
        "modifiedWhen": "2026-10-20T13:08:43.3414616",
        "hasImageAsset": false,
        "scheduledPublishWhen": null,
        "scheduledUnpublishWhen": null
      },
      {
        "contentItemId": 184,
        "languageName": "en",
        "displayName": "Cold brew workshop",
        "latestVersionStatus": "Published",
        "createdWhen": "2026-10-20T13:52:00.5742065",
        "modifiedWhen": "2026-10-20T13:52:00.7551969",
        "hasImageAsset": false,
        "scheduledPublishWhen": null,
        "scheduledUnpublishWhen": null
      },
      {
        "contentItemId": 84,
        "languageName": "en",
        "displayName": "Our first cupping is here!",
        "latestVersionStatus": "Published",
        "createdWhen": "2023-06-13T04:17:44",
        "modifiedWhen": "2026-10-20T13:08:43.3321995",
        "hasImageAsset": false,
        "scheduledPublishWhen": null,
        "scheduledUnpublishWhen": null
      },
      {
        "contentItemId": 106,
        "languageName": "en",
        "displayName": "The art of coffee cupping",
        "latestVersionStatus": "Published",
        "createdWhen": "2023-06-13T04:24:41",
        "modifiedWhen": "2026-10-20T13:08:43.7171427",
        "hasImageAsset": false,
        "scheduledPublishWhen": null,
        "scheduledUnpublishWhen": null
      }
    ]
  }
}
```
<!-- relay:end examples:get-content-item-usage -->

<!-- relay:begin errors:get-content-item-usage -->
### Errors { #get-content-item-usage-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No language named"

    Parameters:

    ```json
    {
      "contentItemId": 19
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "LanguageName is required.",
      "data": null
    }
    ```
<!-- relay:end errors:get-content-item-usage -->

### See also { #get-content-item-usage-see-also }

- [`delete-content-item`](#delete-content-item)
- [`get-content`](#get-content)

---

## get-content-hub-folder

<!-- relay:begin header:get-content-hub-folder -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-content-hub-folder` | [Write](../../concepts/access-tiers.md) | [`GetContentHubFolderResult`](../../reference/types.md#getcontenthubfolderresult) | `GetContentHubFolderCommand` |
<!-- relay:end header:get-content-hub-folder -->

Finds a content hub folder by ID, code name or path, and creates the folders of a path that don't exist yet. The result is the folder ID that [`create-content-item`](#create-content-item) and [`move-content-item`](#move-content-item) take.

<!-- relay:begin parameters:get-content-hub-folder -->
### Parameters { #get-content-hub-folder-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentFolderId` | integer | No | Look up by numeric ID. Mutually exclusive with `codeName` and `folderPath`. |
| `codeName` | string | No | Look up by code name. Mutually exclusive with `contentFolderId` and `folderPath`. |
| `folderPath` | string | No | Slash-separated path, e.g. "Imports/Audio". All missing segments are created. Mutually exclusive with `contentFolderId` and `codeName`. |
| `workspaceName` | string | No | Required when using `folderPath`. Defaults to [`RelayKenticoOptions.DefaultWorkspaceName`](../../configuration.md#defaultworkspacename) when null. |
<!-- relay:end parameters:get-content-hub-folder -->

<!-- relay:begin result:get-content-hub-folder -->
### Result { #get-content-hub-folder-result }

A successful run returns a [GetContentHubFolderResult](../../reference/types.md#getcontenthubfolderresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `contentFolderId` | integer | ID of the folder that was found or created, for [`create-content-item`](#create-content-item) and [`move-content-item`](#move-content-item). |
<!-- relay:end result:get-content-hub-folder -->

### Behavior { #get-content-hub-folder-behavior }

- **Exactly one way to find the folder.** Give `contentFolderId`, `codeName` or `folderPath`. Giving none, or more than one, fails.
- **Paths create folders.** `folderPath` is a slash-separated path of display names, such as `Events/2026`. Every segment that doesn't exist yet is created, so the command is safe to run again: the second run finds the folders and changes nothing. A segment is matched by display name inside its parent. Code names are global, so Relay gives each new folder one built from its whole path, with `/` replaced by `_` (`Events/2026` becomes `Events_2026`), and shortens it with a hash when it would be longer than 49 characters.
- **Workspace.** A path starts at the root of a workspace, so `workspaceName` (a workspace code name) is needed with `folderPath`; it defaults to [`DefaultWorkspaceName`](../../configuration.md#defaultworkspacename). The other two ways don't use it.
- **A Write command.** A path can create folders, so the command needs a Write key even when the folders already exist.
- **Messages.** "Folder 'Events/2026' is ready (ID=5)." for a path, "Found folder '2026' (ID=5)." for the other two ways.

<!-- relay:begin examples:get-content-hub-folder -->
### Examples { #get-content-hub-folder-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Find a folder by path, creating the folders that are missing { #get-content-hub-folder-example-path }

Neither folder existed, so both were created. Running the same command again finds them and changes nothing.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-hub-folder",
      "parameters": {
        "folderPath": "Events/2026",
        "workspaceName": "DancingGoat.DancingGoatEvents"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentHubFolderCommand
    {
        FolderPath = "Events/2026",
        WorkspaceName = "DancingGoat.DancingGoatEvents",
    });
    var data = result.GetData<GetContentHubFolderResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Folder 'Events/2026' is ready (ID=5).",
  "error": null,
  "data": {
    "contentFolderId": 5
  }
}
```

#### Find a folder by its ID { #get-content-hub-folder-example-by-id }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-hub-folder",
      "parameters": {
        "contentFolderId": 5
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentHubFolderCommand
    {
        ContentFolderId = 5,
    });
    var data = result.GetData<GetContentHubFolderResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found folder '2026' (ID=5).",
  "error": null,
  "data": {
    "contentFolderId": 5
  }
}
```

#### Find a folder by its code name { #get-content-hub-folder-example-by-code-name }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-hub-folder",
      "parameters": {
        "codeName": "Events_2026"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentHubFolderCommand
    {
        CodeName = "Events_2026",
    });
    var data = result.GetData<GetContentHubFolderResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found folder '2026' (ID=5).",
  "error": null,
  "data": {
    "contentFolderId": 5
  }
}
```
<!-- relay:end examples:get-content-hub-folder -->

<!-- relay:begin errors:get-content-hub-folder -->
### Errors { #get-content-hub-folder-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No way of identifying the folder"

    Parameters:

    ```json
    {}
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "One of ContentFolderId, CodeName, or FolderPath must be provided.",
      "data": null
    }
    ```

???+ failure "More than one way of identifying the folder"

    Parameters:

    ```json
    {
      "contentFolderId": 5,
      "folderPath": "Events/2026"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Only one of ContentFolderId, CodeName, or FolderPath may be provided.",
      "data": null
    }
    ```

???+ failure "A path without a workspace"

    Parameters:

    ```json
    {
      "folderPath": "Events/2026"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "WorkspaceName is required when using FolderPath. Set it on the command or configure RelayKenticoOptions.DefaultWorkspaceName.",
      "data": null
    }
    ```
<!-- relay:end errors:get-content-hub-folder -->

### See also { #get-content-hub-folder-see-also }

- [`create-content-item`](#create-content-item)
- [`move-content-item`](#move-content-item)

---

## query-reusable-items

<!-- relay:begin header:query-reusable-items -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `query-reusable-items` | [Read](../../concepts/access-tiers.md) | [`QueryItemsResult`](../../reference/types.md#queryitemsresult) | `QueryReusableItemsCommand` |
<!-- relay:end header:query-reusable-items -->

Lists reusable content items of one or more content types and returns only the columns you name. It is the command for finding items: by type, by the value of a field, or by publication status.

<!-- relay:begin parameters:query-reusable-items -->
### Parameters { #query-reusable-items-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentTypeNames` | string[] | Yes | Code names of the content types to include, e.g. `DancingGoat.ArticlePage`. At least one, and at most 25. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `columns` | string[] | Yes | Columns to fetch and return. At least one, and at most 500. Each is a plain column name (letters, digits and underscores) that exists on every content type listed: one of its fields, or a system column such as `ContentItemID`. |
| `whereEquals` | { name: any } | No | Optional column-equality filters, at most 50. All conditions are ANDed together. Each column must also be listed in `columns`, and each value is text, a number, `true`, `false` or `null`. |
| `publishedOnly` | boolean | No | When true, returns the published version of each item, and leaves out items that have none. Defaults to false, which returns each item's latest version, drafts included. |
<!-- relay:end parameters:query-reusable-items -->

<!-- relay:begin result:query-reusable-items -->
### Result { #query-reusable-items-result }

A successful run returns a [QueryItemsResult](../../reference/types.md#queryitemsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `items` | { name: any }[] | One entry per item, mapping each requested column to its value. |
<!-- relay:end result:query-reusable-items -->

### Behavior { #query-reusable-items-behavior }

- **Columns.** Name the columns you want, at least one: the content type's field names, plus system columns such as `ContentItemID`, `ContentItemGUID` and `ContentItemName`. Each item in the answer is an object with just those columns. Reference and tag fields come back as JSON values (arrays of objects), not as strings. A column is a plain name (letters, digits and underscores) that exists on every content type listed, and one that doesn't is refused with a message that names it. Columns of web pages, such as `WebPageItemID`, exist only for web page content types: use [`query-web-page-items`](../web-pages/index.md#query-web-page-items) for those.
- **Filtering.** `whereEquals` keeps the items whose columns equal the values given; the conditions are combined with AND. A column used in `whereEquals` must also be listed in `columns`, otherwise the query is refused. Values are text, numbers, `true`, `false` or `null`.
- **Which version.** By default each item is its latest version, drafts included. With `publishedOnly: true` it is the published version of each item, and items that have none are left out. To find out whether an item is published, run the query twice, once with `publishedOnly`, and compare the `ContentItemID`s.
- **Several types.** `contentTypeNames` can list more than one content type; the answer is the union of their items.
- **Language.** Items come from `languageName`, with Kentico's language fallback.
- **No paging.** The answer holds every matching item, so narrow the query with `whereEquals` when a type has many items.

<!-- relay:begin examples:query-reusable-items -->
### Examples { #query-reusable-items-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### List the cafes { #query-reusable-items-example-cafes }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-reusable-items",
      "parameters": {
        "contentTypeNames": [
          "DancingGoat.Cafe"
        ],
        "columns": [
          "ContentItemID",
          "CafeName",
          "CafeCity"
        ]
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryReusableItemsCommand
    {
        ContentTypeNames = ["DancingGoat.Cafe"],
        Columns = ["ContentItemID", "CafeName", "CafeCity"],
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
        "ContentItemID": 19,
        "CafeName": "Boston coffee place",
        "CafeCity": "Boston"
      },
      {
        "ContentItemID": 30,
        "CafeName": "Chicago",
        "CafeCity": "Chicago"
      },
      {
        "ContentItemID": 81,
        "CafeName": "New York",
        "CafeCity": "New York"
      },
      {
        "ContentItemID": 185,
        "CafeName": "Seattle Coffee Place",
        "CafeCity": "Seattle"
      }
    ]
  }
}
```

#### Filter by a column value { #query-reusable-items-example-filtered }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-reusable-items",
      "parameters": {
        "contentTypeNames": [
          "DancingGoat.Cafe"
        ],
        "columns": [
          "ContentItemID",
          "CafeName",
          "CafeCity"
        ],
        "whereEquals": {
          "CafeCity": "Seattle"
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryReusableItemsCommand
    {
        ContentTypeNames = ["DancingGoat.Cafe"],
        Columns = ["ContentItemID", "CafeName", "CafeCity"],
        WhereEquals = new()
        {
            ["CafeCity"] = JsonSerializer.SerializeToElement("Seattle"),
        },
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
        "ContentItemID": 185,
        "CafeName": "Seattle Coffee Place",
        "CafeCity": "Seattle"
      }
    ]
  }
}
```
<!-- relay:end examples:query-reusable-items -->

<!-- relay:begin errors:query-reusable-items -->
### Errors { #query-reusable-items-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No content types named"

    Parameters:

    ```json
    {
      "columns": [
        "ContentItemID"
      ]
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "ContentTypeNames must list at least one content type.",
      "data": null
    }
    ```

???+ failure "A column the content type does not have"

    Parameters:

    ```json
    {
      "contentTypeNames": [
        "DancingGoat.Cafe"
      ],
      "columns": [
        "ContentItemID",
        "CafeTitle"
      ]
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Column 'CafeTitle' was not found on content type 'DancingGoat.Cafe'.",
      "data": null
    }
    ```

???+ failure "A filter column that is not listed in columns"

    Parameters:

    ```json
    {
      "contentTypeNames": [
        "DancingGoat.Cafe"
      ],
      "columns": [
        "ContentItemID"
      ],
      "whereEquals": {
        "CafeCity": "Seattle"
      }
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "The WhereEquals column 'CafeCity' must also be listed in Columns.",
      "data": null
    }
    ```
<!-- relay:end errors:query-reusable-items -->

### See also { #query-reusable-items-see-also }

- [`query-web-page-items`](../web-pages/index.md#query-web-page-items) for pages
- [`get-content`](#get-content) for one item
- [`search-content`](#search-content) to search text
- [Query content](../../guides/querying.md)

---

## search-content

<!-- relay:begin header:search-content -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `search-content` | [Read](../../concepts/access-tiers.md) | [`SearchContentResult`](../../reference/types.md#searchcontentresult) | `SearchContentCommand` |
<!-- relay:end header:search-content -->

Searches the published items of one content type for some text, without regard to case, across every text, long text and rich text field. It works for reusable content types and for page types.

<!-- relay:begin parameters:search-content -->
### Parameters { #search-content-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentTypeName` | string | Yes | Code name of the content type to search, e.g. `DancingGoat.ArticlePage`. |
| `filter` | string | Yes | The text to look for, up to 1000 characters. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `websiteChannelName` | string | No | Website channel to search, for web page content types. Defaults to [`RelayKenticoOptions.DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname) when null. Not used for reusable content types. |
<!-- relay:end parameters:search-content -->

<!-- relay:begin result:search-content -->
### Result { #search-content-result }

A successful run returns a [SearchContentResult](../../reference/types.md#searchcontentresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `matches` | [SearchContentMatch](../../reference/types.md#searchcontentmatch)[] | The items that matched. |
<!-- relay:end result:search-content -->

### Behavior { #search-content-behavior }

- **What is searched.** Only published items, and only fields of the types `Text`, `LongText` and `RichTextHTML`. The text is matched anywhere in a field; for a rich text field that includes its HTML. It can be up to 1000 characters.
- **What comes back.** One entry per item with at least one matching field. `matchedFields` has the whole value of each field that matched, so it can be long for rich text. `location` is the page's tree path for a page, and `null` for a reusable item. `contentItemId` is a content item ID, also for pages.
- **Pages.** For a page type, `websiteChannelName` says which channel to search (it defaults to [`DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname)), and the command fails when neither is set. It isn't used for reusable types.
- **No matches.** That is a success with an empty list. A content type without any searchable text field also succeeds, with a message that says so.
- **Cost.** The text is matched inside Relay after Kentico returns the published items, so avoid it on a content type with a very large number of items.
- **Language.** `languageName` defaults to [`DefaultLanguageName`](../../configuration.md#defaultlanguagename).

<!-- relay:begin examples:search-content -->
### Examples { #search-content-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Search the published article pages for some text { #search-content-example-pages }

The search covers published items only and every text, long text and rich text field. `location` is the page's tree path.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "search-content",
      "parameters": {
        "contentTypeName": "DancingGoat.ArticlePage",
        "websiteChannelName": "DancingGoatPages",
        "filter": "cold brew"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new SearchContentCommand
    {
        ContentTypeName = "DancingGoat.ArticlePage",
        WebsiteChannelName = "DancingGoatPages",
        Filter = "cold brew",
    });
    var data = result.GetData<SearchContentResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found 1 matching item(s) in 'DancingGoat.ArticlePage'.",
  "error": null,
  "data": {
    "matches": [
      {
        "contentItemId": 182,
        "contentItemName": "ColdBrewAtHome-aqfgqi7s",
        "contentTypeName": "DancingGoat.ArticlePage",
        "location": "/Articles/Cold_brew_at_home",
        "matchedFields": {
          "ArticleTitle": "Cold brew at home: a beginner's guide",
          "ArticlePageSummary": "Everything you need to make cold brew with a jar and a sieve - no special equipment."
        }
      }
    ]
  }
}
```

#### Search the published cafes for some text { #search-content-example-reusable }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "search-content",
      "parameters": {
        "contentTypeName": "DancingGoat.Cafe",
        "filter": "seattle"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new SearchContentCommand
    {
        ContentTypeName = "DancingGoat.Cafe",
        Filter = "seattle",
    });
    var data = result.GetData<SearchContentResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found 1 matching item(s) in 'DancingGoat.Cafe'.",
  "error": null,
  "data": {
    "matches": [
      {
        "contentItemId": 185,
        "contentItemName": "SeattleCoffeePlace-0fxj79os",
        "contentTypeName": "DancingGoat.Cafe",
        "location": null,
        "matchedFields": {
          "CafeName": "Seattle Coffee Place",
          "CafeCity": "Seattle"
        }
      }
    ]
  }
}
```
<!-- relay:end examples:search-content -->

<!-- relay:begin errors:search-content -->
### Errors { #search-content-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No text to look for"

    Parameters:

    ```json
    {
      "contentTypeName": "DancingGoat.Cafe"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Filter is required.",
      "data": null
    }
    ```
<!-- relay:end errors:search-content -->

### See also { #search-content-see-also }

- [`query-web-page-items`](../web-pages/index.md#query-web-page-items) and [`query-reusable-items`](#query-reusable-items) to filter by column values
- [`get-page`](../web-pages/index.md#get-page) and [`get-content`](#get-content) to read a match in full
