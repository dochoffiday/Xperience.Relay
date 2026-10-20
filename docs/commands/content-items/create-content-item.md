# create-content-item

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-content-item` | [Write](../../concepts/access-tiers.md) | [`CreateContentItemResult`](../../reference/types.md#createcontentitemresult) | `CreateContentItemCommand` |
<!-- relay:end header -->

Creates a reusable content item and publishes it. It can put the item in a content hub folder and upload files into its asset fields in the same call.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentTypeName` | string | Yes | Code name of a reusable content type, e.g. `Podcast.Episode`. |
| `displayName` | string | Yes | Display name of the new item. Longer names are cut to 100 characters. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `workspaceName` | string | No | Defaults to [`RelayKenticoOptions.DefaultWorkspaceName`](../../configuration.md#defaultworkspacename) when null. |
| `contentFolderId` | integer | No | Content hub folder to place the item in after creation. Use the [`get-content-hub-folder`](get-content-hub-folder.md) command first to resolve or create the target folder and get its ID. |
| `fields` | { name: any } | No | Plain field values by field name (text, numbers, booleans, dates, ...). They are checked against the content type before anything is created. |
| `linkedItemFields` | { name: guid[] } | No | Linked-items fields. Maps field name to a list of ContentItemGUIDs. The field must be a content item reference field. |
| `tagFields` | { name: guid[] } | No | Tag fields. Maps field name to a list of tag GUIDs. The field must be a tag (taxonomy) field. |
| `assets` | [RelayAsset](../../reference/types.md#relayasset)[] | No | Optional binary assets to upload into asset fields. Each entry maps to one field. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [CreateContentItemResult](../../reference/types.md#createcontentitemresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `contentItemGuid` | guid | GUID of the new item. |
| `contentItemId` | integer | ID of the new item. |
<!-- relay:end result -->

## Behavior

- **Published at once.** There is no draft step: the item is created and published, and the answer says so ("Created and published ..."). If Kentico creates the item but declines to publish it, the result is a failure that names the new item's ID.
- **Reusable types only.** `contentTypeName` must be a reusable content type; page types go through [`create-web-page`](../web-pages/create-web-page.md). An unknown type fails with "Content type '...' was not found."
- **Workspace.** `workspaceName` is the workspace's code name (for example `DancingGoat.DancingGoatEvents`). When it is left out Relay uses [`DefaultWorkspaceName`](../../configuration.md#defaultworkspacename), and the command fails if that isn't set either.
- **Folder.** Give `contentFolderId` to move the new item into a content hub folder once it is created. [`get-content-hub-folder`](get-content-hub-folder.md) finds or creates the folder and returns its ID.
- **Name.** `displayName` is cut to 100 characters. Kentico makes the item's code name from it.
- **Fields.** They are checked the way [`create-web-page`](../web-pages/create-web-page.md) checks them: unknown fields and values that don't fit are refused, all problems at once, and nothing is created. `linkedItemFields` takes content item GUIDs, `tagFields` takes tag GUIDs and `assets` takes Base64 files. See [Upload files](../../guides/uploading-assets.md).
- **The result.** `contentItemId` is the ID the other content item commands take. `contentItemGuid` is what reference fields of other items use.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Create an event in a content hub folder

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

### Create an item with a tag field

`CafePromotion` is a tag field, so it takes tag GUIDs (see [`get-tags`](../taxonomies/get-tags.md)).

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

### Create an item with an uploaded file

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
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

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
<!-- relay:end errors -->

## See also

- [`update-content-item`](update-content-item.md) to change it later
- [`get-content-hub-folder`](get-content-hub-folder.md) to find a folder
- [Upload files](../../guides/uploading-assets.md)
