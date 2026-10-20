# Upload files

Files (images, documents, audio) go into **asset fields** of content items and pages. Relay sends them as Base64 text inside the same JSON as everything else, so there is no separate upload endpoint to learn.

## How it works

A file is a `RelayAsset` with three parts:

| Part | Meaning |
|---|---|
| `fieldName` | The asset field to put the file in, such as `ImageFile`. |
| `fileName` | The file's name with its extension, such as `cold-brew.png`. Kentico uses it for the asset's name and extension. |
| `base64` | The file's bytes, Base64-encoded. |

Four commands take a list of them in `assets`: [`create-content-item`](../commands/content-items/create-content-item.md), [`update-content-item`](../commands/content-items/update-content-item.md), [`create-web-page`](../commands/web-pages/create-web-page.md) and [`update-web-page`](../commands/web-pages/update-web-page.md). One call can fill several asset fields.

On the server, Relay decodes each file to a temporary file, hands it to Kentico as an asset, and deletes the temporary files afterwards, whatever the outcome.

## Create an item with a file

```json
{
  "contentTypeName": "DancingGoat.Image",
  "displayName": "Cold brew diagram",
  "workspaceName": "DancingGoat.DancingGoatLtd",
  "fields": { "ImageShortDescription": "How cold brew is made" },
  "assets": [
    { "fieldName": "ImageFile", "fileName": "cold-brew.png", "base64": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==" }
  ]
}
```

That file is a one-pixel PNG, to keep the example short. In C#, read a real file and encode it:

```csharp
using Xperience.Relay.Contracts.Commands;

var result = await relay.ExecuteAsync(new CreateContentItemCommand
{
    ContentTypeName = "DancingGoat.Image",
    DisplayName = "Cold brew diagram",
    WorkspaceName = "DancingGoat.DancingGoatLtd",
    Assets =
    [
        new RelayAsset
        {
            FieldName = "ImageFile",
            FileName = "cold-brew.png",
            Base64 = Convert.ToBase64String(await File.ReadAllBytesAsync("cold-brew.png")),
        },
    ],
});
```

## What Kentico stores

Reading the item back with [`get-content`](../commands/content-items/get-content.md) shows the asset field as the JSON text Kentico stores:

```json
{
  "Original": { "Identifier": "1d4f67c8-2cee-414b-8320-2fef5ed3d836", "Name": "cold-brew.png", "Extension": ".png", "Size": 70, "Width": 1, "Height": 1 },
  "OptimizationParameters": { "OutputExtension": "webp", "Quality": 70, "WidthLimit": null, "HeightLimit": null },
  "Variants": {},
  "Name": "cold-brew.webp",
  "Extension": ".webp",
  "Size": 96,
  "Width": 1,
  "Height": 1
}
```

(Abridged.) Kentico keeps the original and also stores an optimized version. Here the field's settings convert images to WebP, so the stored name is `cold-brew.webp` although the upload was a PNG. What happens depends on the optimization settings of the field.

## Rules and limits

- **The field has to be an asset field.** Putting a file into any other field is refused before anything changes: `Field 'ImageShortDescription' is not an asset field (it is text), so it can't be set from Assets.`
- **One file per field.** If two entries name the same field, the last one wins.
- **Incomplete entries are ignored.** An entry that lacks its `fieldName`, `fileName` or `base64` is skipped without an error.
- **Size.** The file travels inside the request body, and Base64 makes it about a third bigger. The body is limited to [`MaxRequestBodySizeBytes`](../configuration.md#maxrequestbodysizebytes), 256 MB by default, which fits files of up to about 190 MB. Proxies and hosting platforms in front of the site can have limits of their own, often lower. A large upload also needs a longer client timeout (see [Call Relay from .NET](dotnet-client.md#timeouts-and-handlers)).
- **Memory.** The request body, the Base64 text and the decoded bytes are all held in memory while the file is processed, so avoid sending very large files at once.
- **State.** On an update, the item's published or draft state is kept: a published item is published again with the new file. See [Drafts and publishing](../concepts/drafts-and-publishing.md).

## Files that are already uploaded

Two commands work on a file that is already in an asset field, without sending it again:

- [`rename-asset`](../commands/assets/rename-asset.md) changes the file's name.
- [`reoptimize-asset`](../commands/assets/reoptimize-asset.md) runs the file through Kentico's optimization again, after the field's settings changed.

Both find the file on the server, hand it to Kentico again, and keep the item's state, so nothing but the new name travels over the wire.
