# Xperience.Relay

A command-dispatch framework for remotely driving Kentico Xperience operations (move pages, fetch
page/content data, ...) that's deployed *inside* a live Xperience web app and called over HTTP.

## Why this exists

Production Xperience by Kentico instances are commonly deployed to Kentico's SaaS environment,
which does not allow external .NET applications to connect to the database directly (confirmed via
Kentico's own docs: "You cannot connect to Xperience applications deployed to the SaaS environment
from external .NET applications"). That rules out a typical external console app or library that
talks to the DB directly.

Instead, Relay's Kentico-aware handlers run *inside* the deployed web app and are exposed over a
thin, secured HTTP API. A separate lightweight client (not yet built -- see below) calls that API
remotely. This is why the packages are split the way they are: only the packages that actually need
the Kentico SDK depend on it, so a remote caller's client library never needs it.

## Packages

| Package | Depends on Kentico SDK? | Purpose |
|---|---|---|
| `Xperience.Relay.Contracts` | No | Command/result/envelope types shared by every other package. |
| `Xperience.Relay.Core` | No | Command dispatch (`IRelayDispatcher`), pipeline behaviors, verb registry. Self-rolled, no MediatR dependency. |
| `Xperience.Relay.Kentico` | Yes (`Kentico.Xperience.Core` 31.5.4) | Handlers that actually call Kentico APIs (`IWebPageManager`, `IContentItemManager`, ...). Deployed as part of the live Xperience app. |
| `Xperience.Relay.Hosting` | No | ASP.NET Core endpoints (`/commands`, `/batch`, `/verbs`) exposing the dispatcher over HTTP, with API-key auth. |
| `Xperience.Relay.Client` | No | Lightweight remote caller (`RelayClient`) -- no Kentico SDK dependency, just HTTP + the `Contracts` types. Resolves each command's verb from its `[RelayCommand]` attribute and posts to `/commands`/`/batch`, or reads `/verbs`. |

## Command model

Each command is a plain class implementing `IRelayCommand`, tagged with `[RelayCommand("verb-name")]`
(see `src/Xperience.Relay.Contracts/Commands/`). `RelayVerbRegistry` discovers these via reflection
at startup. A handler implements `IRelayCommandHandler<TCommand>` and is resolved via DI by
`RelayDispatcher`, which threads execution through any registered `IRelayPipelineBehavior`s
(logging, validation, retry, etc. -- none are built yet, the seam just exists).

Results travel as `RelayCommandResult { Success, Message, Error, Data }`. `Data` is `object?` rather
than a generic type parameter, deliberately -- since everything crosses the wire as JSON anyway,
adding a second generic type parameter through the dispatcher would buy nothing. Query-style verbs
(`get-page`, `get-content-info`, ...) put their DTO in `Data`; callers deserialize based on which
verb they called.

## Verbs implemented so far

- **`move`** -- `MoveCommand { WebPageId, ParentWebPageId }`. Deliberately has no `Order` field --
  it always appends as the last child. Reordering is a future, separate `sort` command's job, not
  this one's. Also deliberately assumes the parent already exists; this command does not create
  folders along the way.
- **`get-page-info`** / **`get-page`** -- `GetPageInfoCommand`/`GetPageCommand`, returning
  `WebPageInfo`/`WebPageData`. Split this way (rather than always returning full field data) because
  the cheap, system-fields-only version is what a caller needs to resolve a path to an ID before
  sending a `move`, without paying for fetching a page's full content fields.
- **`get-content-info`** / **`get-content`** -- same split, for reusable content items, returning
  `ContentInfo`/`ContentData`.

`GetPageCommand`/`GetContentCommand` always compose their `*Info` counterpart internally and layer
field data on top, rather than duplicating the system-fields lookup.

## Non-obvious implementation decisions

**Why `MoveCommandHandler` doesn't use `IWebsiteChannelContext`.** Kentico's docs show
`IWebPageManager` instances created via `webPageManagerFactory.Create(websiteChannelContext.WebsiteChannelID, userId)`
-- but that assumes you're inside a request already scoped to a known channel (e.g. rendering a
page on that channel's site). Relay commands arrive channel-agnostic: a bare `WebPageId` with no
known channel. So `MoveCommandHandler` resolves the channel ID first via a channel-agnostic content
item query (`ContentItemQueryBuilder().ForContentTypes(...)`, reading
`IWebPageContentQueryDataContainer.WebPageItemWebsiteChannelID`), *then* creates the channel-scoped
manager. `GetPageInfoCommandHandler` uses the same query-based approach for the same reason, rather
than `IWebPageManager.GetWebPageMetadata` (which would need a channel-scoped manager up front).

**Why `LanguageName` exists on some commands but not others.** `IWebPageManager.GetWebPageMetadata`
and `IContentItemManager.GetContentItemMetadata` are language-neutral (no language parameter) --
confirmed by reflecting over the real compiled `Kentico.Xperience.Core` SDK, not by guessing. But
since the page-info handler is implemented via a content item query instead (see above), and any
`ContentItemQueryBuilder` query requires `.InLanguage(...)`, `GetPageInfoCommand`, `GetPageCommand`,
and `GetContentCommand` all carry a nullable `LanguageName` (defaulting to
`RelayKenticoOptions.DefaultLanguageName`). `GetContentInfoCommand` doesn't need it, since it goes
through `IContentItemManager.GetContentItemMetadata` directly.

**Why `move`'s sibling order is computed, not requested.** Since the command has no `Order` field
by design, `MoveCommandHandler.GetNextChildOrderAsync` queries the target parent's existing children,
takes `Max(WebPageItemOrder) + 1` (or `0` if there are none), and passes that into
`MoveWebPageParameters`. This is a best-effort assumption about `Order`'s semantics (0-based,
sequential) -- not verified against a live Xperience instance, since none was available while
building this.

**Why `ContentInfo.WorkspaceName` holds a numeric ID, not a friendly name.** `ContentItemMetadata`
exposes `WorkspaceId`, but no public API surface for resolving a workspace's display name was found
in `Kentico.Xperience.Core` 31.5.4 (confirmed by reflecting over every type containing "Workspace" in
the compiled assemblies -- none exist in the public API at this SDK version). If a later SDK version
adds one, `GetContentInfoCommandHandler.FetchContentInfoAsync` is the place to use it.

**Why the API key check in `RelayApiKeyEndpointFilter` hashes both sides before comparing.**
`CryptographicOperations.FixedTimeEquals` is constant-time only when both inputs are the same
length; comparing raw UTF-8 bytes of a wrong-length guess could still leak length via early
allocation/comparison differences. Hashing both sides first (SHA-256) normalizes the length before
the fixed-time comparison.

**Why `RelayCommandResult.Data` isn't generic.** Considered threading a `TResult` generic parameter
through `IRelayCommandHandler<TCommand, TResult>` and `RelayDispatcher`, but since results always
serialize to JSON for the HTTP layer anyway, that would add real complexity (the dispatcher's
reflection-based invocation in `RelayDispatcher.InvokeHandlerAsync` would need a second generic
dimension) for no benefit a caller couldn't get more simply by deserializing `Data` into the DTO
they expect for the verb they called.

## What's not built yet

- **Integration/live testing** -- everything here is compile-verified against the real
  `Kentico.Xperience.Core` 31.5.4 assembly (via a throwaway reflection probe used while building this,
  not checked in), but none of it has run against an actual Xperience database. Treat the Kentico
  handlers as "should work per the documented API," not "verified."
- **`sort` command** -- explicitly deferred; `move` always appends last.
- A `query`/`insert`/`update`/`delete` verb family beyond the `get-*` commands already here.
- Per-request user attribution for Kentico's "Modified By" auditing. Right now every command runs as
  one fixed `RelayKenticoOptions.ServiceAccountUserName`. Note that even per-request attribution
  would *not* enforce that user's actual Kentico permissions -- Kentico's management API
  (`IWebPageManagerFactory.Create(channelId, userId)`, `IContentItemManagerFactory.Create(userId)`)
  documents that the `userId` is used for audit attribution only, not permission checks.
- LICENSE file (this repo is public on GitHub; license choice not yet decided).
- Nothing in this repo has been pushed to GitHub yet -- it's local-only as of this writing.

## Eventual integration with a consuming Xperience site

Once packaged, a consuming Xperience web app would add `Xperience.Relay.Kentico`
and `Xperience.Relay.Hosting` as normal `PackageReference`s -- no different from how it already
references `Kentico.Xperience.Core`. At that point: configure `RelayKenticoOptions` (service account
username, default language) and `RelayHostingOptions` (API key) in the host's DI setup, and call
`endpoints.MapRelayEndpoints()` in routing configuration. That integration work hasn't started.
