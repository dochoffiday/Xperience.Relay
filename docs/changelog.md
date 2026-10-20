# Changelog

Versions are tagged in the repository as `v<major>.<minor>.<patch>`, and a tag publishes the five NuGet packages at that version. This page lists the changes that matter to callers; for anything older than 0.4.0 see the tags and commit history on GitHub.

## 0.6.0 (2026-10-20)

**Upgrading from 0.5.0:** a few commands now fail where they used to report success (deleting or moving something that doesn't exist), and `get-content` now returns the latest version of an item, not the published one. Check any caller that relied on either.

### Documentation

- This documentation site: a page for each of the 41 commands, concept pages, guides and reference pages. The parameter tables, results, examples and error samples of the command pages are generated from the code by `tools/Xperience.Relay.Docs`, so they can't drift from it.
- XML documentation for every command parameter and result field, which the packages now ship (IntelliSense).

### Fixed

- A request without a `parameters` property, natural for commands that take none, such as `get-taxonomies`, returned HTTP 500. It is now treated as `{}`.
- `get-page` now works. It failed for every page.
- `get-page-info` now returns the page's content type, and `channelName` is the channel's code name (it held a host name).
- `get-content` now returns the item's latest version, drafts included, as `get-page` does (it returned only the published version).
- `query-web-page-items` and `query-reusable-items` fail with a clear message when `contentTypeNames` or `columns` is empty.
- Commands that reported success when nothing happened now fail with a message that says so: `delete-content-item` for an item or language variant that doesn't exist, `delete-web-page` for a missing language variant, `delete-tag` for a tag that doesn't exist, and `move-content-item` for IDs that don't exist or belong to pages.
- `move-web-page` fails with a clear message for a parent that doesn't exist (it threw a `NullReferenceException`), and its documentation no longer says the page is placed last: Kentico places it first.
- `move-tag` refuses a parent from another taxonomy, and a move under the tag itself or one of its own child tags. `create-tag` checks its parent too.
- `query-sql` no longer allows `SELECT ... INTO` a table (a temporary `#table` is still fine), and says so when a query returns no result set.
- `create-custom-object` and `update-custom-object` refuse a column the object type doesn't have, and an empty `fields`.
- Clearer messages: an unknown content type is "not found" (not "not reusable"), `rename-asset` and `reoptimize-asset` refuse a field that isn't an asset field, `get-content-item-usage` says `LanguageName` is required, `get-form-submissions` says when the form doesn't exist, and `create-tag` names the taxonomy it couldn't find.
- `delete-contacts` says "across all batches" for a `batchSize` of 0.
- An exception thrown by a handler before its first `await` is reported as itself, no longer as a `TargetInvocationException`.

## 0.5.0 (2026-10-20)

### Added

- `query-web-page-items` has `includeStatus`, which adds each page's `WebPageStatus` and `WebPageIsPublished` from the same query.
- `query-web-page-items` and `query-reusable-items` have `publishedOnly`, to read the published version instead of the latest.
- Create and update commands check the fields against the content type before changing anything, and report every problem at once.

### Changed

- `update-web-page`, `update-content-item`, `update-slug`, `rename-asset` and `reoptimize-asset` share one draft-based edit flow. An item that was live is published again with the change; a draft or unpublished item stays as it was. When a change isn't live, the message says so.
- `publish-*` and `unpublish-*` read the item's state first and report what Kentico actually did. They are safe to repeat, an unpublished item can be published again, and they never answer "published" when nothing was published.
- A failed edit leaves nothing behind: a draft made for it is discarded and Kentico's cached copy is dropped.

### Fixed

- `rename-asset` and `reoptimize-asset` could lose the original file of an asset.

## 0.4.0 (2026-10-19)

- API keys. Requests are authenticated with a key sent in `X-Relay-Api-Key`, and every command has a minimum access tier (Read, Write or Admin). Keys are created and revoked in the **Relay API keys** application in the Xperience admin; only a hash of each key is stored.
