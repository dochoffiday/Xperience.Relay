# Web pages

Pages live in the content tree of a website channel. Every page command takes the page's ID (`webPageId`): [`create-web-page`](create-web-page.md) returns it, and [`get-page-info`](get-page-info.md) or [`query-web-page-items`](query-web-page-items.md) find it for pages that already exist.

A page has one variant per language, and publishing is per variant. Changes go through drafts the way they do in the admin UI: see [Drafts and publishing](../../concepts/drafts-and-publishing.md). [Create, edit and publish a page](../../guides/page-lifecycle.md) walks through the commands in order.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`create-web-page`](create-web-page.md) | Write | Creates a new web page under a parent web page. |
| [`update-web-page`](update-web-page.md) | Write | Updates one or more fields on an existing web page. |
| [`update-slug`](update-slug.md) | Write | Changes the URL slug of a web page. |
| [`move-web-page`](move-web-page.md) | Write | Moves an existing web page under a different parent, or to the root of its channel. |
| [`publish-web-page`](publish-web-page.md) | Write | Publishes a web page language variant: a draft is published, and an unpublished page gets a draft made first. |
| [`unpublish-web-page`](unpublish-web-page.md) | Write | Unpublishes a web page language variant. |
| [`delete-web-page`](delete-web-page.md) | Write | Deletes a language variant of a web page. |
| [`get-page-info`](get-page-info.md) | Read | Fetches a web page's system fields (content type, tree path, parent ID, ...) without its content-type-specific field data. |
| [`get-page`](get-page.md) | Read | Fetches a web page's system fields plus the field values of its latest version (drafts included). |
| [`query-web-page-items`](query-web-page-items.md) | Read | Queries web pages of one or more content types, returning only the requested columns. |
<!-- relay:end commands -->
