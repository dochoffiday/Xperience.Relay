# Assets

Files uploaded into asset fields of reusable content items. Uploading is part of [`create-content-item`](../content-items/create-content-item.md), [`update-content-item`](../content-items/update-content-item.md) and the page commands (see [Upload files](../../guides/uploading-assets.md)); the two commands here work on a file that is already uploaded and send nothing over the wire.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`rename-asset`](rename-asset.md) | Write | Renames the file in an existing asset field of a reusable content item, without sending the file over the wire. |
| [`reoptimize-asset`](reoptimize-asset.md) | Write | Runs an existing asset field's file through Kentico's asset optimization again (format conversion, quality, size limits), without sending the file over the wire. |
<!-- relay:end commands -->
