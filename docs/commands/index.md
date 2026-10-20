# Commands

Relay has 41 commands, each with its own page. A command is identified by its **verb**, takes its parameters as JSON, and answers with a [result](../reference/types.md#relaycommandresult). The **Access** column is the lowest [API key tier](../concepts/access-tiers.md) that may run it.

## [Web pages](web-pages/index.md)

| Command | Access | What it does |
|---|---|---|
| [`create-web-page`](web-pages/create-web-page.md) | Write | Creates a new web page under a parent web page. |
| [`update-web-page`](web-pages/update-web-page.md) | Write | Updates one or more fields on an existing web page. |
| [`update-slug`](web-pages/update-slug.md) | Write | Changes the URL slug of a web page. |
| [`move-web-page`](web-pages/move-web-page.md) | Write | Moves an existing web page under a different parent, or to the root of its channel. |
| [`publish-web-page`](web-pages/publish-web-page.md) | Write | Publishes a web page language variant: a draft is published, and an unpublished page gets a draft made first. |
| [`unpublish-web-page`](web-pages/unpublish-web-page.md) | Write | Unpublishes a web page language variant. |
| [`delete-web-page`](web-pages/delete-web-page.md) | Write | Deletes a language variant of a web page. |
| [`get-page-info`](web-pages/get-page-info.md) | Read | Fetches a web page's system fields (content type, tree path, parent ID, ...) without its content-type-specific field data. |
| [`get-page`](web-pages/get-page.md) | Read | Fetches a web page's system fields plus the field values of its latest version (drafts included). |
| [`query-web-page-items`](web-pages/query-web-page-items.md) | Read | Queries web pages of one or more content types, returning only the requested columns. |

## [Content items](content-items/index.md)

| Command | Access | What it does |
|---|---|---|
| [`create-content-item`](content-items/create-content-item.md) | Write | Creates a new reusable content item and publishes it, optionally uploading one or more binary assets into asset fields. |
| [`update-content-item`](content-items/update-content-item.md) | Write | Updates one or more fields on an existing reusable content item. |
| [`move-content-item`](content-items/move-content-item.md) | Write | Moves one or more reusable content items into a content hub folder. |
| [`publish-content-item`](content-items/publish-content-item.md) | Write | Publishes a reusable content item language variant: a draft is published, and an unpublished item gets a draft made first. |
| [`unpublish-content-item`](content-items/unpublish-content-item.md) | Write | Unpublishes a reusable content item language variant. |
| [`delete-content-item`](content-items/delete-content-item.md) | Write | Deletes a language variant of a reusable content item. |
| [`get-content-info`](content-items/get-content-info.md) | Read | Fetches a reusable content item's system fields (content type, language, workspace, ...) without its content-type-specific field data. |
| [`get-content`](content-items/get-content.md) | Read | Fetches a reusable content item's system fields plus the field values of its latest version (drafts included). |
| [`get-content-item-usage`](content-items/get-content-item-usage.md) | Read | Finds the content items that directly reference a content item's language variant (the "used by" list in the admin UI). |
| [`get-content-hub-folder`](content-items/get-content-hub-folder.md) | Write | Looks up a content hub folder by ID, codename, or path. |
| [`query-reusable-items`](content-items/query-reusable-items.md) | Read | Queries reusable content items of one or more content types, returning only the requested columns. |
| [`search-content`](content-items/search-content.md) | Read | Searches the published items of one content type for text, case-insensitively, across every text, long text and rich text field. |

## [Assets](assets/index.md)

| Command | Access | What it does |
|---|---|---|
| [`rename-asset`](assets/rename-asset.md) | Write | Renames the file in an existing asset field of a reusable content item, without sending the file over the wire. |
| [`reoptimize-asset`](assets/reoptimize-asset.md) | Write | Runs an existing asset field's file through Kentico's asset optimization again (format conversion, quality, size limits), without sending the file over the wire. |

## [Taxonomies and tags](taxonomies/index.md)

| Command | Access | What it does |
|---|---|---|
| [`create-taxonomy`](taxonomies/create-taxonomy.md) | Write | Creates a taxonomy. |
| [`update-taxonomy`](taxonomies/update-taxonomy.md) | Write | Changes a taxonomy's title and/or description. |
| [`delete-taxonomy`](taxonomies/delete-taxonomy.md) | Write | Deletes a taxonomy. |
| [`get-taxonomies`](taxonomies/get-taxonomies.md) | Read | Lists all taxonomies. |
| [`create-tag`](taxonomies/create-tag.md) | Write | Creates a tag in a taxonomy, at the root or under another tag. |
| [`update-tag`](taxonomies/update-tag.md) | Write | Changes a tag's title and/or description. |
| [`move-tag`](taxonomies/move-tag.md) | Write | Moves a tag under another tag, or to the root of its taxonomy, at a given position. |
| [`delete-tag`](taxonomies/delete-tag.md) | Write | Deletes a tag. |
| [`get-tags`](taxonomies/get-tags.md) | Read | Returns tags matching the given filter. |

## [Custom objects](custom-objects/index.md)

| Command | Access | What it does |
|---|---|---|
| [`create-custom-object`](custom-objects/create-custom-object.md) | Admin | Creates an object of any registered Kentico object type (typically a custom module class) by setting the given columns and inserting it, so Kentico's events and cache invalidation run as usual. |
| [`update-custom-object`](custom-objects/update-custom-object.md) | Admin | Updates an object of any registered Kentico object type by ID, changing only the given columns, through Kentico so its events and cache invalidation run as usual. |
| [`delete-custom-object`](custom-objects/delete-custom-object.md) | Admin | Deletes an object of any registered Kentico object type by ID, through Kentico so its events and cache invalidation run as usual. |
| [`query-custom-objects`](custom-objects/query-custom-objects.md) | Admin | Reads objects of any registered Kentico object type (typically a custom module class). |

## [Forms and contacts](forms-and-contacts/index.md)

| Command | Access | What it does |
|---|---|---|
| [`get-form-submissions`](forms-and-contacts/get-form-submissions.md) | Read | Reads the submissions of a Kentico form (BizForm). |
| [`delete-contacts`](forms-and-contacts/delete-contacts.md) | Admin | Bulk-deletes the contacts that match a SQL condition, through Kentico's contact deletion service. |

## [SQL](sql/index.md)

| Command | Access | What it does |
|---|---|---|
| [`query-sql`](sql/query-sql.md) | Admin | Executes a read-only SQL query against the Xperience database. |
| [`execute-sql`](sql/execute-sql.md) | Admin | Runs SQL against the Xperience database in two steps. |
