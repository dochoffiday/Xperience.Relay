# reoptimize-asset

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `reoptimize-asset` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `ReoptimizeAssetCommand` |
<!-- relay:end header -->

Runs the file in an asset field through Kentico's asset optimization again (format conversion, quality, size limits), without sending the file again.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `fieldName` | string | Yes | Name of the asset field, e.g. `ImageFile`. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **When to use it.** After the optimization settings of a field have changed in Kentico, so files that were uploaded earlier get the new settings.
- **No upload.** The file is already on the server. Relay finds it and hands it to Kentico again with a fresh identifier, so Kentico processes it the way it processes an upload.
- **State is kept.** The change goes through a draft like any edit: a published item is published again, and a draft or unpublished item stays that way. The table in [`update-web-page`](../web-pages/update-web-page.md) applies as it is.
- **Formats that are skipped.** `.ico` files can't be handled by the image pipeline. They are skipped, and the answer says so instead of failing.
- **Which field.** The field has to be an asset field, and the item has to have a file in it for that language.
- **A missing file.** If the file is no longer in the server's storage the result says so, and the item is left untouched: no draft is made.
- **Reusable items only.** The command works on reusable content items, not on pages.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Run the file through Kentico's asset optimization again

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "reoptimize-asset",
      "parameters": {
        "contentItemId": 186,
        "fieldName": "ImageFile"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new ReoptimizeAssetCommand
    {
        ContentItemId = 186,
        FieldName = "ImageFile",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Re-optimized asset 'ImageFile' on content item 186.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A field that is not an asset field"

    Parameters:

    ```json
    {
      "contentItemId": 186,
      "fieldName": "ImageShortDescription"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Field 'ImageShortDescription' is not an asset field (it is text).",
      "data": null
    }
    ```

???+ failure "An item that does not exist"

    Parameters:

    ```json
    {
      "contentItemId": 99999,
      "fieldName": "ImageFile"
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

- [`rename-asset`](rename-asset.md)
- [`get-content`](../content-items/get-content.md) to read the asset field
