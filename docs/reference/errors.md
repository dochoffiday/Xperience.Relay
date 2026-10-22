# Errors

There are two layers of errors. HTTP errors say the request never reached a command. Everything else is a **failed result**: HTTP `200` with `success` set to `false` and the reason in `error`.

## HTTP errors

| Status | Meaning | What to do |
|---|---|---|
| `401` | The API key is missing or unknown. No body. | Check the header name and the key. A deleted key looks like this too. |
| `403` | The key's tier is lower than the command's. | Use a key with the tier the message names, or a different command. |
| `400` | The body isn't valid JSON. | Fix the request. |
| `413` | The body is bigger than [`MaxRequestBodySizeBytes`](../configuration.md#maxrequestbodysizebytes). | Send less per request, or raise the limit. |
| `500` | Something outside a command failed. | Look in the site's log. |

## Failed results

The messages below are the ones you will meet most, grouped by cause. The text of a message can include the IDs and names from your request.

### Setup

| Message | What it means and what to do |
|---|---|
| `RelayKenticoOptions.ServiceAccountUserName must be configured before relay commands that modify content can run.` | Set [`ServiceAccountUserName`](../configuration.md#serviceaccountusername). |
| `Relay service account user '...' was not found.` | The configured user doesn't exist in Kentico. Create it, or fix the name. |
| `WebsiteChannelName is required. Set it on the command or configure RelayKenticoOptions.DefaultWebsiteChannelName.` | Pass `websiteChannelName`, or set [`DefaultWebsiteChannelName`](../configuration.md#defaultwebsitechannelname). |
| `WorkspaceName is required...` | Pass `workspaceName`, or set [`DefaultWorkspaceName`](../configuration.md#defaultworkspacename). |
| `Channel '...' was not found.` | The channel code name is wrong. It is the code name, not the display name. |

### The request

| Message | What it means and what to do |
|---|---|
| `Unknown verb '...'.` | The verb is misspelled, or the site runs a version without it. [`GET /verbs`](http-api.md#get-verbs) lists what it supports. |
| `Could not deserialize parameters for verb '...': ...` | A parameter has the wrong type. The message names the property (`Path: $.webPageId`). |
| `Parameters for verb '...' resolved to null.` | `parameters` was `null`. Send an object, or leave it out. |
| `... is required.`, `... must not be empty.`, `... must list at least one ...` | A required parameter is missing. The message names it. |
| `No fields provided — nothing to update.` | An update command had nothing to change. |
| `One of ContentFolderId, CodeName, or FolderPath must be provided.` | [`get-content-hub-folder`](../commands/content-items/index.md#get-content-hub-folder) needs exactly one. Giving two gives `Only one of ... may be provided.` |
| `At least one filter (...) is required.` | [`get-tags`](../commands/taxonomies/index.md#get-tags) needs a filter. |
| `LanguageName is required.` | [`get-content-item-usage`](../commands/content-items/index.md#get-content-item-usage) has no default language. |

### Not found

| Message | What it means and what to do |
|---|---|
| `Web page N was not found.` | No page has that ID. |
| `Web page N (the new parent) was not found.` | [`move-web-page`](../commands/web-pages/index.md#move-web-page): the target parent doesn't exist. |
| `Content item N was not found.` | No content item has that ID. |
| `... was not found in that language.` | The page or item exists but has no variant in the language. Relay can't add a language; see [Defaults and languages](../concepts/defaults-and-languages.md). |
| `Content item N is not a reusable content item. Use ... for web pages.` | You passed a page's content item ID to a content item command. Use the page command. |
| `Taxonomy N was not found.`, `Tag N was not found.` | No taxonomy or tag has that ID. |
| `Content type '...' was not found.` | The content type's code name is wrong. |
| `Object type '...' was not found.` | No Kentico object type is registered under that name. |
| `Form '...' was not found. ...` | The form class name is wrong. It is shown in the admin under the form's properties. |
| `'type' with ID N was not found.` | [Custom object](../commands/custom-objects/index.md) commands: no object has that ID. |

### Fields that don't fit

A create or update command checks its fields against the content type before it changes anything, and lists **every** problem in one message:

| Message | What it means |
|---|---|
| `Field 'X' was not found on content type 'Y'.` | The type has no such field (names aren't case sensitive). |
| `Field 'X' expects a date, but got '...'.` | The value can't be read as a date. Similar messages exist for whole numbers, numbers, `true` or `false`, GUIDs and time spans. |
| `Field 'X' allows at most N characters, but the value has M.` | The text is too long. |
| `Field 'X' can't be null.` | The field must have a value. |
| `Field 'X' is not a content item reference field (it is text), so it can't be set from LinkedItemFields.` | Linked items, tags and files each go only into a field of that kind. |

### State

| Message | What it means and what to do |
|---|---|
| `... is published and also has a draft with unpublished changes, and Kentico won't unpublish it while that draft exists. Publish or discard the draft first.` | Publish the draft with `publish-*`, or discard it in the admin. |
| `Kentico would not create a draft of ... to edit.` | Kentico refused to start an edit, for example because the page has a scheduled publish. The site's event log says why. |
| `Kentico did not save the changes to ...` | Kentico refused the saved change. See the event log. |
| `The changes to ... were saved as a draft, but Kentico did not publish them, so the live version is unchanged.` | The edit is in a draft, and only the final publish failed. Publish it, or look in the event log. |
| `Kentico did not publish ...` / `Kentico did not unpublish ...` | Kentico declined. See the event log. |
| `Web page '...' was created (ID=N) but could not be published.` | The page exists as a draft. Publish it with [`publish-web-page`](../commands/web-pages/index.md#publish-web-page) instead of creating it again. |

### SQL

| Message | What it means |
|---|---|
| `SQL parsing error: ...` | The SQL doesn't parse. |
| `'DELETE' statements are not allowed in this context.` | The statement type isn't allowed by this command. |
| `'OPENROWSET' is not allowed.` | One of the [blocked names](../concepts/guardrails.md#how-sql-is-checked). |
| `The relay API key table is protected and can't be queried or changed through relay.` | The SQL mentions the key table. |
| `'SELECT ... INTO' creates a table, so it is not allowed here ...` | `query-sql` is read-only. |
| `The query returned no result set. End it with a SELECT.` | `query-sql` returned nothing to show. |
| `Query failed: ...` | SQL Server's own error: a missing column, a timeout, a constraint. |

### Tags

| Message | What it means |
|---|---|
| `A tag can't be moved under itself or under one of its own child tags.` | The new parent is the tag or one of its descendants. |
| `Tag N belongs to a different taxonomy, so tag M can't be moved under it.` | A tag's parent has to be in the same taxonomy. |

## Kentico's own errors

When Kentico itself throws, Relay passes the exception on as a failed result:

```text
Unhandled exception executing verb 'create-web-page': [InvalidOperationException] Content type with ID 5665 is not allowed as a child of web page with ID 0.
```

The part in brackets is the exception's type, and the text after it is **Kentico's message**, which is usually clear. Common ones:

| Kentico says | Why |
|---|---|
| `Content type with ID N is not allowed as a child of web page with ID M.` | The parent doesn't allow that content type under it. Pages are limited by their parent's allowed child types, in [`create-web-page`](../commands/web-pages/index.md#create-web-page) and [`move-web-page`](../commands/web-pages/index.md#move-web-page). |
| `The web page with ID N does not exist.` | A page ID in a parameter such as `redirectToWebPageId` is wrong. |
| `The content folder with ID N does not exist.` | [`move-content-item`](../commands/content-items/index.md#move-content-item) was given a folder ID that doesn't exist. |

The exception is also written to the site's log, with its stack trace, which is where to look when the message isn't enough.
