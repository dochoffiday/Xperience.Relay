# get-content

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-content` | [Read](../../concepts/access-tiers.md) | [`ContentData`](../../reference/types.md#contentdata) | `GetContentCommand` |
<!-- relay:end header -->

Reads a reusable content item's system fields, the same ones [`get-content-info`](get-content-info.md) returns, together with the field values of its latest version. The latest version includes drafts, so you see what editors see in the admin UI, not what the live site shows.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [ContentData](../../reference/types.md#contentdata) in `data`:

| Field | Type | Description |
|---|---|---|
| `contentItemId` | integer | ID of the content item. |
| `contentItemGuid` | guid | GUID of the content item. |
| `name` | string | The item's code name. |
| `contentType` | string | Code name of the item's content type, e.g. `DancingGoat.Image`. |
| `languageName` | string | Empty from [`get-content-info`](get-content-info.md) (the item's system fields don't vary by language); the requested language from [`get-content`](get-content.md). |
| `workspaceName` | string | The workspace's numeric ID, as text. Kentico's public API doesn't expose a workspace's name. |
| `contentFolderPath` | string | Reserved. Not filled in yet, so currently always null. |
| `fields` | { name: any } | Field values of the item's latest version, by field name, as JSON (text, numbers, dates, ...). Asset, tag and reference fields come as the JSON text Kentico stores in them. Only the content type's own fields are listed: fields that come from reusable field schemas are not included. |
<!-- relay:end result -->

## Behavior

- **Field values.** `fields` is keyed by the content type's own field names. Text is a string, numbers and booleans are JSON values, dates are ISO strings without a time zone (`"2026-11-14T10:00:00"`), and a value that isn't set is `null`. Reference, tag and asset fields come as the JSON text Kentico stores in them: a string that contains JSON. Fields that come from reusable field schemas aren't included.
- **Published version.** To read what is live, use [`query-reusable-items`](query-reusable-items.md) with `publishedOnly`.
- **Language.** `languageName` defaults to [`DefaultLanguageName`](../../configuration.md#defaultlanguagename), and the answer reports the language you asked for. An item that has no variant in it comes back with the values of the language Kentico falls back to.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Read an item with its field values

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

### Read an item with an asset field

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
<!-- relay:end examples -->

<!-- relay:begin errors -->
<!-- relay:end errors -->

## See also

- [`get-content-info`](get-content-info.md) when you don't need the fields
- [`update-content-item`](update-content-item.md) to change them
- [`query-reusable-items`](query-reusable-items.md) to read many items at once
