# Design notes

Decisions behind the shape of Relay, and the limits that follow from them. They matter when you extend the library, or wonder why something works the way it does.

## Why the handlers run inside the web app

An Xperience site in the SaaS environment can't be reached by external .NET applications, so a tool outside it can't use Kentico's APIs or the database. Relay's handlers therefore live in the deployed app, behind a thin HTTP API, and only the packages that need the Kentico SDK depend on it. See [How Relay works](concepts/how-it-works.md).

## Why commands don't carry a channel

Kentico's docs create an `IWebPageManager` with `webPageManagerFactory.Create(websiteChannelContext.WebsiteChannelID, userId)`, which assumes a request that is already scoped to a channel, such as rendering a page of that channel. Relay commands arrive channel-agnostic: a bare `webPageId` says nothing about the channel.

So the page handlers (move, delete, update, publish, update-slug) first read the page's own row with `IInfoProvider<WebPageItemInfo>`, take `WebPageItemWebsiteChannelID` from it, and only then create the channel-scoped manager. Only the commands that create or list pages, which have nothing to read the channel from, take a `websiteChannelName`.

## Why `languageName` is on some commands and not others

`IWebPageManager.GetWebPageMetadata` and `IContentItemManager.GetContentItemMetadata` are language-neutral. `get-content-info` calls the second directly, so it has no language, and its `languageName` is empty. `get-page-info`, `get-page` and `get-content` are built on content item queries, and every `ContentItemQueryBuilder` query requires `.InLanguage(...)`, so they carry an optional `languageName` that defaults to [`DefaultLanguageName`](configuration.md#defaultlanguagename).

## Why a moved page goes first

`MoveWebPageParameters` takes only the page and the new parent, with no position. Relay passes those two and lets Kentico place the page, which puts it first among its new siblings. There is no sort command.

## Why `workspaceName` in `get-content-info` is a number

`ContentItemMetadata` exposes `WorkspaceId`, but Kentico's public API has no way to turn a workspace ID into its name. Relay returns the ID as text so callers can still match it with the admin UI. If a later SDK version adds a lookup, `GetContentInfoCommandHandler` is the place to use it.

## Why edits go through drafts, and Relay asks first

Kentico's `TryCreateDraft`, `TryUpdateDraft`, `TryPublish` and `TryUnpublish` answer a plain `false` to anything they won't do, including "there was nothing to do". A caller can't tell a harmless no-op from a refusal, and an earlier version of Relay reported success for a publish that hadn't happened.

So Relay reads the item's state first (`ContentVersionReader`: the status of the latest version, and whether any version is published), then follows the plan that state calls for, and reports what happened. The rules live in two small, unit-tested classes, `PublishingFlow` and `DraftEditFlow`. They are described on [Drafts and publishing](concepts/drafts-and-publishing.md).

Two details are worth knowing:

- **Discard only when it is safe.** A draft made over a published version can be discarded to restore the item. A draft made over an unpublished item can't (Kentico turns the item into an `InitialDraft` in place), so Relay doesn't try.
- **Failures clear Kentico's cache.** Kentico caches the latest version of an item. After a failed write that cache can still hold the refused value, and the next edit would write it again, so Relay drops the cached copies (`FailedWriteCacheCleaner`) when a step throws.

## Why fields are checked first

Kentico drops a field it doesn't know about without saying so, stores linked items or tags as JSON text in whatever field it is told to, and fails on a value it can't store only halfway through a write, after the draft exists. `ContentFieldChecker` and `FieldValueChecker` check names and values against the content type, including its reusable field schemas, before anything is created or changed, and report every problem at once.

## Why queries read the latest version

The query commands ask Kentico for the preview version by default, so drafts show up the way they do for editors, and `publishedOnly` asks for the live version instead. `includeStatus` takes the version status from the same query, so telling which pages are published costs nothing extra.

## Why API keys are an unsalted SHA-256 hash

Keys are 256 bits of randomness, so there is nothing to brute-force and no reason for a salt or a slow password hash. A request hashes the presented key and looks it up in an in-memory copy of the key table, which Kentico's cache invalidation refreshes when a key is added or deleted. Revocation is immediate, and requests never touch the database. "Last used" is written straight to the table, at most every five minutes per key, rather than through the info object, because updating the object would invalidate that cache on every request.

## Why `get-content-hub-folder` is a Write command

With `folderPath` it creates any missing folders, so it can't be treated as a read.

The handler uses `IInfoProvider<ContentFolderInfo>` instead of `IContentFolderManager.Get`, because `IContentFolderManager` creates one folder at a time under a known parent and has no "ensure this path" operation. The handler splits the path and walks the segments, checking each by parent and display name. `Get` is a global code-name lookup, so it would silently match a same-named folder elsewhere in the tree and send the walk down the wrong branch. Each new folder gets a code name built from its whole path (`Events/2026` becomes `Events_2026`), because code names are global and a raw segment can contain characters a code name can't. A name that would pass the 50-character limit is shortened and ends in a hash, to stay unique.

## Why `data` isn't generic

Threading a result type through `IRelayCommandHandler<TCommand, TResult>` and `RelayDispatcher` would add real complexity, since the dispatcher invokes handlers by reflection, for no benefit: results are JSON on the wire anyway, and a caller already knows which command it sent. `RelayCommandResult.GetData<T>()` does the cast, or reads the JSON, or round-trips through JSON as a last resort.

## Why files are Base64

Every command travels as plain JSON in the envelope. Rather than design a separate multipart upload path for the few commands that carry files, the bytes go in `RelayAsset.Base64` and the handler decodes them to temporary files, wraps each in Kentico's asset metadata, and deletes them in a `finally` block. Several asset fields can be filled in one call.

## Limits that follow

- **No way to add a language.** Writes act on a variant that exists; creating a variant isn't a command yet.
- **One service account.** Every change is attributed to one fixed user. Kentico's management API uses a user ID for attribution only and never checks permissions, so per-request attribution wouldn't make a caller's own Kentico permissions apply.
- **No chaining in a batch.** `as` is reserved for naming a result so a later command could use it, and does nothing yet.
- **No paging** in the query commands, and no operators beyond equality in `whereEquals`.
- **No built-in pipeline behaviors.** The seam exists; nothing is registered by default.
- **Two ways to see reference values.** `get-page` and `get-content` return reference, tag and asset fields as the JSON text Kentico stores; the query commands embed them as JSON. Both are kept for compatibility.
- **`contentFolderPath`** in `get-content` and `get-content-info` is not filled in yet.
