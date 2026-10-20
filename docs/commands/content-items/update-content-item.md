# update-content-item

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateContentItemCommand` |
<!-- relay:end header -->

Changes fields of a reusable content item in one language, through a draft, and leaves the item in the state it found it in. It is the reusable-item counterpart of [`update-web-page`](../web-pages/update-web-page.md).

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `fields` | { name: any } | No | Plain field values by field name (text, numbers, booleans, dates, ...). |
| `linkedItemFields` | { name: guid[] } | No | Linked-items fields. Maps field name to a list of ContentItemGUIDs. The field must be a content item reference field. An empty list clears the field. |
| `tagFields` | { name: guid[] } | No | Tag fields. Maps field name to a list of tag GUIDs. The field must be a tag (taxonomy) field. An empty list clears the field. |
| `assets` | [RelayAsset](../../reference/types.md#relayasset)[] | No | Optional binary assets to upload into asset fields. Each entry maps to one field. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **State is kept.** A published item is published again with the change, and a draft or unpublished item stays that way. The table in [`update-web-page`](../web-pages/update-web-page.md) applies as it is, and so do the messages ("Saved as a draft; it is not published.").
- **Failures leave nothing behind.** If a step fails, a draft made for the edit is discarded again and Kentico's cached copy is dropped, exactly as for pages.
- **Checks.** Fields are checked against the content type before anything changes, all problems at once.
- **Clearing a field.** An empty list in `linkedItemFields` or `tagFields` clears that field.
- **Reusable items only.** The content item of a web page is refused with "Content item N is not a reusable content item. Use update-web-page for web pages." An item with no variant in `languageName` fails with "was not found in that language".
- **Something to change.** At least one of `fields`, `linkedItemFields`, `tagFields` and `assets` must be given.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Update an item that is published

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
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

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
<!-- relay:end errors -->

## See also

- [`update-web-page`](../web-pages/update-web-page.md)
- [`publish-content-item`](publish-content-item.md) to put a draft live
- [`get-content`](get-content.md) to read the fields back
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)
