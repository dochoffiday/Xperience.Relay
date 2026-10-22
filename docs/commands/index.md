# Commands

Relay has 41 commands in 7 areas. Each area has one page with a section for every command: its parameters, result, behavior, examples and errors. A command is identified by its **verb**, takes its parameters as JSON, and answers with a [result](../reference/types.md#relaycommandresult). The **Access** column is the lowest [API key tier](../concepts/access-tiers.md) that may run it.

## [Web pages](web-pages/index.md)

| Command | Access | What it does |
|---|---|---|
| [`create-web-page`](web-pages/index.md#create-web-page) | Write | Creates a new web page under a parent web page. |
| [`update-web-page`](web-pages/index.md#update-web-page) | Write | Updates one or more fields on an existing web page. |
| [`update-slug`](web-pages/index.md#update-slug) | Write | Changes the URL slug of a web page. |
| [`move-web-page`](web-pages/index.md#move-web-page) | Write | Moves an existing web page under a different parent, or to the root of its channel. |
| [`publish-web-page`](web-pages/index.md#publish-web-page) | Write | Publishes a web page language variant: a draft is published, and an unpublished page gets a draft made first. |
| [`unpublish-web-page`](web-pages/index.md#unpublish-web-page) | Write | Unpublishes a web page language variant. |
| [`delete-web-page`](web-pages/index.md#delete-web-page) | Write | Deletes a language variant of a web page. |
| [`get-page-info`](web-pages/index.md#get-page-info) | Read | Fetches a web page's system fields (content type, tree path, parent ID, ...) without its content-type-specific field data. |
| [`get-page`](web-pages/index.md#get-page) | Read | Fetches a web page's system fields plus the field values of its latest version (drafts included). |
| [`query-web-page-items`](web-pages/index.md#query-web-page-items) | Read | Queries web pages of one or more content types, returning only the requested columns. |

## [Content items](content-items/index.md)

| Command | Access | What it does |
|---|---|---|
| [`create-content-item`](content-items/index.md#create-content-item) | Write | Creates a new reusable content item and publishes it, optionally uploading one or more binary assets into asset fields. |
| [`update-content-item`](content-items/index.md#update-content-item) | Write | Updates one or more fields on an existing reusable content item. |
| [`move-content-item`](content-items/index.md#move-content-item) | Write | Moves one or more reusable content items into a content hub folder. |
| [`publish-content-item`](content-items/index.md#publish-content-item) | Write | Publishes a reusable content item language variant: a draft is published, and an unpublished item gets a draft made first. |
| [`unpublish-content-item`](content-items/index.md#unpublish-content-item) | Write | Unpublishes a reusable content item language variant. |
| [`delete-content-item`](content-items/index.md#delete-content-item) | Write | Deletes a language variant of a reusable content item. |
| [`get-content-info`](content-items/index.md#get-content-info) | Read | Fetches a reusable content item's system fields (content type, language, workspace, ...) without its content-type-specific field data. |
| [`get-content`](content-items/index.md#get-content) | Read | Fetches a reusable content item's system fields plus the field values of its latest version (drafts included). |
| [`get-content-item-usage`](content-items/index.md#get-content-item-usage) | Read | Finds the content items that directly reference a content item's language variant (the "used by" list in the admin UI). |
| [`get-content-hub-folder`](content-items/index.md#get-content-hub-folder) | Write | Looks up a content hub folder by ID, codename, or path. |
| [`query-reusable-items`](content-items/index.md#query-reusable-items) | Read | Queries reusable content items of one or more content types, returning only the requested columns. |
| [`search-content`](content-items/index.md#search-content) | Read | Searches the published items of one content type for text, case-insensitively, across every text, long text and rich text field. |

## [Assets](assets/index.md)

| Command | Access | What it does |
|---|---|---|
| [`rename-asset`](assets/index.md#rename-asset) | Write | Renames the file in an existing asset field of a reusable content item, without sending the file over the wire. |
| [`reoptimize-asset`](assets/index.md#reoptimize-asset) | Write | Runs an existing asset field's file through Kentico's asset optimization again (format conversion, quality, size limits), without sending the file over the wire. |

## [Taxonomies and tags](taxonomies/index.md)

| Command | Access | What it does |
|---|---|---|
| [`create-taxonomy`](taxonomies/index.md#create-taxonomy) | Write | Creates a taxonomy. |
| [`update-taxonomy`](taxonomies/index.md#update-taxonomy) | Write | Changes a taxonomy's title and/or description. |
| [`delete-taxonomy`](taxonomies/index.md#delete-taxonomy) | Write | Deletes a taxonomy. |
| [`get-taxonomies`](taxonomies/index.md#get-taxonomies) | Read | Lists all taxonomies. |
| [`create-tag`](taxonomies/index.md#create-tag) | Write | Creates a tag in a taxonomy, at the root or under another tag. |
| [`update-tag`](taxonomies/index.md#update-tag) | Write | Changes a tag's title and/or description. |
| [`move-tag`](taxonomies/index.md#move-tag) | Write | Moves a tag under another tag, or to the root of its taxonomy, at a given position. |
| [`delete-tag`](taxonomies/index.md#delete-tag) | Write | Deletes a tag. |
| [`get-tags`](taxonomies/index.md#get-tags) | Read | Returns tags matching the given filter. |

## [Custom objects](custom-objects/index.md)

| Command | Access | What it does |
|---|---|---|
| [`create-custom-object`](custom-objects/index.md#create-custom-object) | Admin | Creates an object of any registered Kentico object type (typically a custom module class) by setting the given columns and inserting it, so Kentico's events and cache invalidation run as usual. |
| [`update-custom-object`](custom-objects/index.md#update-custom-object) | Admin | Updates an object of any registered Kentico object type by ID, changing only the given columns, through Kentico so its events and cache invalidation run as usual. |
| [`delete-custom-object`](custom-objects/index.md#delete-custom-object) | Admin | Deletes an object of any registered Kentico object type by ID, through Kentico so its events and cache invalidation run as usual. |
| [`query-custom-objects`](custom-objects/index.md#query-custom-objects) | Admin | Reads objects of any registered Kentico object type (typically a custom module class). |

## [Forms and contacts](forms-and-contacts/index.md)

| Command | Access | What it does |
|---|---|---|
| [`get-form-submissions`](forms-and-contacts/index.md#get-form-submissions) | Read | Reads the submissions of a Kentico form (BizForm). |
| [`delete-contacts`](forms-and-contacts/index.md#delete-contacts) | Admin | Bulk-deletes the contacts that match a SQL condition, through Kentico's contact deletion service. |

## [SQL](sql/index.md)

| Command | Access | What it does |
|---|---|---|
| [`query-sql`](sql/index.md#query-sql) | Admin | Executes a read-only SQL query against the Xperience database. |
| [`execute-sql`](sql/index.md#execute-sql) | Admin | Runs SQL against the Xperience database in two steps. |
