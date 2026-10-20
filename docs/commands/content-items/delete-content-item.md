# delete-content-item

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteContentItemCommand` |
<!-- relay:end header -->

Deletes a reusable content item in one language. The item goes to the recycle bin, where it can be restored from the admin UI.

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

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **Recycle bin.** Relay has no gone-for-good deletion for content items.
- **The language.** The command deletes the language variant you name. When that is the item's only variant, the item itself is deleted.
- **References aren't checked.** Kentico deletes an item even when other items refer to it, and those references are left pointing at nothing. Run [`get-content-item-usage`](get-content-item-usage.md) first to see what would be affected.
- **Missing items.** An item that doesn't exist, an item that is a web page's content item (use [`delete-web-page`](../web-pages/delete-web-page.md)), and an item with no variant in `languageName` all fail with a message that says so. Relay never reports a deletion that didn't happen.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Delete an item

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
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

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
<!-- relay:end errors -->

## See also

- [`get-content-item-usage`](get-content-item-usage.md) to see what refers to an item
- [`unpublish-content-item`](unpublish-content-item.md) to take an item offline instead
- [`delete-web-page`](../web-pages/delete-web-page.md)
