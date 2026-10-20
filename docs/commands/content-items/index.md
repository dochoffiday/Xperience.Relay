# Content items

Reusable content items live in the content hub, in a workspace and optionally in a folder. Every content item command takes the item's ID (`contentItemId`): [`create-content-item`](create-content-item.md) returns it, and [`query-reusable-items`](query-reusable-items.md) or [`search-content`](search-content.md) find it for items that already exist.

An item has one variant per language, and changes go through drafts like pages do: see [Drafts and publishing](../../concepts/drafts-and-publishing.md). The commands for pages and for items are separate, and an ID of the wrong kind is refused with a pointer to the right command.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`create-content-item`](create-content-item.md) | Write | Creates a new reusable content item and publishes it, optionally uploading one or more binary assets into asset fields. |
| [`update-content-item`](update-content-item.md) | Write | Updates one or more fields on an existing reusable content item. |
| [`move-content-item`](move-content-item.md) | Write | Moves one or more reusable content items into a content hub folder. |
| [`publish-content-item`](publish-content-item.md) | Write | Publishes a reusable content item language variant: a draft is published, and an unpublished item gets a draft made first. |
| [`unpublish-content-item`](unpublish-content-item.md) | Write | Unpublishes a reusable content item language variant. |
| [`delete-content-item`](delete-content-item.md) | Write | Deletes a language variant of a reusable content item. |
| [`get-content-info`](get-content-info.md) | Read | Fetches a reusable content item's system fields (content type, language, workspace, ...) without its content-type-specific field data. |
| [`get-content`](get-content.md) | Read | Fetches a reusable content item's system fields plus the field values of its latest version (drafts included). |
| [`get-content-item-usage`](get-content-item-usage.md) | Read | Finds the content items that directly reference a content item's language variant (the "used by" list in the admin UI). |
| [`get-content-hub-folder`](get-content-hub-folder.md) | Write | Looks up a content hub folder by ID, codename, or path. |
| [`query-reusable-items`](query-reusable-items.md) | Read | Queries reusable content items of one or more content types, returning only the requested columns. |
| [`search-content`](search-content.md) | Read | Searches the published items of one content type for text, case-insensitively, across every text, long text and rich text field. |
<!-- relay:end commands -->
