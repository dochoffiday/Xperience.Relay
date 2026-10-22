---
toc_depth: 2
---

# Assets

Files uploaded into asset fields of reusable content items. Uploading is part of [`create-content-item`](../content-items/index.md#create-content-item), [`update-content-item`](../content-items/index.md#update-content-item) and the page commands (see [Upload files](../../guides/uploading-assets.md)); the two commands here work on a file that is already uploaded and send nothing over the wire.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`rename-asset`](#rename-asset) | Write | Renames the file in an existing asset field of a reusable content item, without sending the file over the wire. |
| [`reoptimize-asset`](#reoptimize-asset) | Write | Runs an existing asset field's file through Kentico's asset optimization again (format conversion, quality, size limits), without sending the file over the wire. |
<!-- relay:end commands -->

---

## rename-asset

<!-- relay:begin header:rename-asset -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `rename-asset` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `RenameAssetCommand` |
<!-- relay:end header:rename-asset -->

Renames the file in an asset field of a reusable content item, without sending the file again.

<!-- relay:begin parameters:rename-asset -->
### Parameters { #rename-asset-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `fieldName` | string | Yes | Name of the asset field, e.g. `ImageFile`. |
| `assetName` | string | Yes | New filename including extension, e.g. "report-2024.pdf". |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:rename-asset -->

<!-- relay:begin result:rename-asset -->
### Result { #rename-asset-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:rename-asset -->

### Behavior { #rename-asset-behavior }

- **No upload.** The file is already on the server. Relay finds it, hands it to Kentico again under the new name, and Kentico records the rename.
- **State is kept.** The change goes through a draft like any edit: a published item is published again, and a draft or unpublished item stays that way. The table in [`update-web-page`](../web-pages/index.md#update-web-page) applies as it is.
- **The extension.** `assetName` should include the extension. The file goes through Kentico's asset processing again, so an image can end up stored in Kentico's optimized format (for example `.webp`) whatever extension the name has; read the field back with [`get-content`](../content-items/index.md#get-content) to see what was stored.
- **Which field.** The field has to be an asset field ("Field '...' is not an asset field (it is text)."), and the item has to have a file in it for that language.
- **A missing file.** If the file is no longer in the server's storage the result says so, and the item is left untouched: no draft is made.
- **Reusable items only.** The command works on reusable content items, not on pages.

<!-- relay:begin examples:rename-asset -->
### Examples { #rename-asset-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Rename the file in an asset field { #rename-asset-example-png }

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
<!-- relay:end examples:rename-asset -->

<!-- relay:begin errors:rename-asset -->
### Errors { #rename-asset-errors }

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
<!-- relay:end errors:rename-asset -->

### See also { #rename-asset-see-also }

- [`reoptimize-asset`](#reoptimize-asset)
- [`get-content`](../content-items/index.md#get-content) to read the asset field
- [Upload files](../../guides/uploading-assets.md)

---

## reoptimize-asset

<!-- relay:begin header:reoptimize-asset -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `reoptimize-asset` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `ReoptimizeAssetCommand` |
<!-- relay:end header:reoptimize-asset -->

Runs the file in an asset field through Kentico's asset optimization again (format conversion, quality, size limits), without sending the file again.

<!-- relay:begin parameters:reoptimize-asset -->
### Parameters { #reoptimize-asset-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `fieldName` | string | Yes | Name of the asset field, e.g. `ImageFile`. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters:reoptimize-asset -->

<!-- relay:begin result:reoptimize-asset -->
### Result { #reoptimize-asset-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:reoptimize-asset -->

### Behavior { #reoptimize-asset-behavior }

- **When to use it.** After the optimization settings of a field have changed in Kentico, so files that were uploaded earlier get the new settings.
- **No upload.** The file is already on the server. Relay finds it and hands it to Kentico again with a fresh identifier, so Kentico processes it the way it processes an upload.
- **State is kept.** The change goes through a draft like any edit: a published item is published again, and a draft or unpublished item stays that way. The table in [`update-web-page`](../web-pages/index.md#update-web-page) applies as it is.
- **Formats that are skipped.** `.ico` files can't be handled by the image pipeline. They are skipped, and the answer says so instead of failing.
- **Which field.** The field has to be an asset field, and the item has to have a file in it for that language.
- **A missing file.** If the file is no longer in the server's storage the result says so, and the item is left untouched: no draft is made.
- **Reusable items only.** The command works on reusable content items, not on pages.

<!-- relay:begin examples:reoptimize-asset -->
### Examples { #reoptimize-asset-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Run the file through Kentico's asset optimization again { #reoptimize-asset-example-png }

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
<!-- relay:end examples:reoptimize-asset -->

<!-- relay:begin errors:reoptimize-asset -->
### Errors { #reoptimize-asset-errors }

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
<!-- relay:end errors:reoptimize-asset -->

### See also { #reoptimize-asset-see-also }

- [`rename-asset`](#rename-asset)
- [`get-content`](../content-items/index.md#get-content) to read the asset field
