# rename-asset

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `rename-asset` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `RenameAssetCommand` |
<!-- relay:end header -->

Renames the file in an asset field of a reusable content item, without sending the file again.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `fieldName` | string | Yes | Name of the asset field, e.g. `ImageFile`. |
| `assetName` | string | Yes | New filename including extension, e.g. "report-2024.pdf". |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **No upload.** The file is already on the server. Relay finds it, hands it to Kentico again under the new name, and Kentico records the rename.
- **State is kept.** The change goes through a draft like any edit: a published item is published again, and a draft or unpublished item stays that way. The table in [`update-web-page`](../web-pages/update-web-page.md) applies as it is.
- **The extension.** `assetName` should include the extension. The file goes through Kentico's asset processing again, so an image can end up stored in Kentico's optimized format (for example `.webp`) whatever extension the name has; read the field back with [`get-content`](../content-items/get-content.md) to see what was stored.
- **Which field.** The field has to be an asset field ("Field '...' is not an asset field (it is text)."), and the item has to have a file in it for that language.
- **A missing file.** If the file is no longer in the server's storage the result says so, and the item is left untouched: no draft is made.
- **Reusable items only.** The command works on reusable content items, not on pages.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Rename the file in an asset field

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "rename-asset",
      "parameters": {
        "contentItemId": 186,
        "fieldName": "ImageFile",
        "assetName": "cold-brew-diagram.png"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new RenameAssetCommand
    {
        ContentItemId = 186,
        FieldName = "ImageFile",
        AssetName = "cold-brew-diagram.png",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Renamed asset 'ImageFile' on content item 186 to 'cold-brew-diagram.png'.",
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
      "fieldName": "ImageShortDescription",
      "assetName": "x.png"
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
<!-- relay:end errors -->

## See also

- [`reoptimize-asset`](reoptimize-asset.md)
- [`get-content`](../content-items/get-content.md) to read the asset field
- [Upload files](../../guides/uploading-assets.md)
