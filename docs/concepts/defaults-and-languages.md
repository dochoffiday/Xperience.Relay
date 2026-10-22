# Defaults and languages

Many commands have parameters you can leave out, because Relay fills them in from the site's configuration. This page lists what is filled in, and explains how languages behave.

## Defaults from configuration

| Parameter | Used by | Falls back to |
|---|---|---|
| `languageName` | Nearly every command that touches content | [`DefaultLanguageName`](../configuration.md#defaultlanguagename) (`en` unless you change it) |
| `workspaceName` | `create-content-item`, `get-content-hub-folder` (with a path) | [`DefaultWorkspaceName`](../configuration.md#defaultworkspacename) |
| `websiteChannelName` | `create-web-page`, `query-web-page-items`, `search-content` (for page types) | [`DefaultWebsiteChannelName`](../configuration.md#defaultwebsitechannelname) |

When a parameter is left out and there is no default to fall back to, the command fails with a message that says what to set: `WebsiteChannelName is required. Set it on the command or configure RelayKenticoOptions.DefaultWebsiteChannelName.`

One exception: [`get-content-item-usage`](../commands/content-items/index.md#get-content-item-usage) has no language default. `languageName` is required there, because it says which variant of the item to look at.

Names, not display names. `workspaceName` and `websiteChannelName` are **code names** (such as `DancingGoat.DancingGoatEvents` and `DancingGoatPages`), and `languageName` is a language's code name (`en`, `es`).

## Languages

A page or item has one **variant** per language. Each variant has its own content and its own versions, so each has its own [status](drafts-and-publishing.md).

### Writing

- **`create-web-page` and `create-content-item`** create the page or item in one language, `languageName`.
- **Every other write command** acts on a variant that has to exist. Asking for a language the item has no variant in fails with "was not found in that language" and changes nothing.
- **Other languages are never touched.** Editing, publishing, unpublishing or deleting `en` leaves `es` as it was.
- Relay has **no command that adds a language variant** to an existing page or item. Add translations in the admin UI.

### Reading

- **`get-page`, `get-content` and the query commands** read the language you name, and Kentico's language fallback applies: an item with no variant in that language comes back with the values of the language it falls back to.
- **`get-page-info`** reports the language you asked for and does not check that the page has a variant in it.
- **`get-content-info`** has no language, because the system fields don't vary by language. Its `languageName` is empty.
- **`includeStatus` and `includeUrls`** describe the variant a row comes from, which may be the fallback language's.

### Deleting

`delete-web-page` and `delete-content-item` delete the variant you name. When it is the last variant, the page or item itself is deleted.

## The service account

Besides defaults, one setting is needed to change content: [`ServiceAccountUserName`](../configuration.md#serviceaccountusername), the Kentico user that Relay's changes are attributed to. It isn't a default you can override per command.
