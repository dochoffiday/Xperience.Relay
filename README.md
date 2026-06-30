# Xperience.Relay

A command-dispatch framework for remotely driving Kentico Xperience operations (move pages, create
content items, update web pages, fetch page/content data, ...) that's deployed *inside* a live
Xperience web app and called over HTTP.

## Why this exists

Production Xperience by Kentico instances are commonly deployed to Kentico's SaaS environment,
which does not allow external .NET applications to connect to the database directly (confirmed via
Kentico's own docs: "You cannot connect to Xperience applications deployed to the SaaS environment
from external .NET applications"). That rules out a typical external console app or library that
talks to the DB directly.

Instead, Relay's Kentico-aware handlers run *inside* the deployed web app and are exposed over a
thin, secured HTTP API. A separate lightweight client calls that API remotely. This is why the
packages are split the way they are: only the packages that actually need the Kentico SDK depend on
it, so a remote caller's client library never needs it.

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
verb they called. `RelayCommandResult.GetData<T>()` handles this: it tries a direct cast first,
falls back to `JsonElement.Deserialize<T>` if the result arrived over the wire, and round-trips
through JSON as a last resort.

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
- **`create-content-hub-folder`** -- `CreateContentHubFolderCommand { FolderPath, WorkspaceName? }`.
  Walks a slash-separated path (e.g. `"Imports/Audio"`) and creates any missing segments. Idempotent
  -- safe to call even if the path already exists. Returns `CreateContentHubFolderResult { ContentFolderId }`
  so the caller can feed the ID straight into a `create-content-item` command.
- **`create-content-item`** -- `CreateContentItemCommand { ContentTypeName, DisplayName, LanguageName?,
  WorkspaceName?, ContentFolderId?, Fields?, Asset? }`. Creates a new reusable content item, publishes
  it, and optionally moves it into a content hub folder. Scalar fields go in `Fields`
  (`Dictionary<string, JsonElement>`); a binary file goes in `Asset` as a Base64-encoded string (see
  below). Returns `CreateContentItemResult { ContentItemGuid, ContentItemId }`.
- **`update-web-page`** -- `UpdateWebPageCommand { WebPageId, LanguageName?, Fields?, LinkedItemFields? }`.
  Updates fields on an existing web page. Scalar fields go in `Fields`; linked-item fields (where
  the value is a list of content item GUIDs) go in `LinkedItemFields` -- pass an empty list to clear
  a field. Preserves the page's current published/draft state: if it was published the draft is
  immediately re-published after the update; if it was a draft the update stays as a draft.

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
manager. `GetPageInfoCommandHandler` and `UpdateWebPageCommandHandler` use the same query-based
approach for the same reason.

**Why `LanguageName` exists on some commands but not others.** `IWebPageManager.GetWebPageMetadata`
and `IContentItemManager.GetContentItemMetadata` are language-neutral (no language parameter) --
confirmed by reflecting over the real compiled `Kentico.Xperience.Core` SDK, not by guessing. But
since the page-info handler is implemented via a content item query instead (see above), and any
`ContentItemQueryBuilder` query requires `.InLanguage(...)`, `GetPageInfoCommand`, `GetPageCommand`,
`GetContentCommand`, `UpdateWebPageCommand`, and `CreateContentItemCommand` all carry a nullable
`LanguageName` (defaulting to `RelayKenticoOptions.DefaultLanguageName`). `GetContentInfoCommand`
doesn't need it, since it goes through `IContentItemManager.GetContentItemMetadata` directly.

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

**Why `create-content-item` sends binary files as Base64.** All relay commands travel as plain JSON
inside `RelayCommandEnvelope.Parameters`. Rather than designing a separate multipart upload path
just for asset-bearing commands, the asset bytes are Base64-encoded in `RelayAsset.Base64` and
decoded server-side to a temp file. The handler writes the bytes to a temp path, wraps it in
Kentico's `ContentItemAssetMetadataWithSource`, and deletes the temp file in a `finally` block
regardless of outcome.

**Why `update-web-page` reads `ContentItemCommonDataVersionStatus` before writing.** The handler
needs to know whether to re-publish after the update. It reads `VersionStatus` from the content
query result rather than calling a separate metadata API, so the channel ID and published state are
resolved in a single round-trip. If the page was published the handler calls `TryPublish` after
`TryUpdateDraft`; if it was a draft the update stays as a draft.

**Why `create-content-hub-folder` walks segments rather than posting the full path.** Kentico's
`IContentFolderManager` creates one folder at a time under a known parent. There's no "ensure path"
API, so the handler splits the slash-separated path, calls `Get` for each segment (to check
existence), and calls `Create` only for missing ones. This makes the command idempotent at each
segment boundary, not just at the leaf.

## What's not built yet

- **Integration/live testing** -- everything here is compile-verified against the real
  `Kentico.Xperience.Core` 31.5.4 assembly (via a throwaway reflection probe used while building this,
  not checked in), but none of it has run against an actual Xperience database. Treat the Kentico
  handlers as "should work per the documented API," not "verified."
- **`sort` command** -- explicitly deferred; `move` always appends last.
- Per-request user attribution for Kentico's "Modified By" auditing. Right now every command runs as
  one fixed `RelayKenticoOptions.ServiceAccountUserName`. Note that even per-request attribution
  would *not* enforce that user's actual Kentico permissions -- Kentico's management API
  (`IWebPageManagerFactory.Create(channelId, userId)`, `IContentItemManagerFactory.Create(userId)`)
  documents that the `userId` is used for audit attribution only, not permission checks.
- LICENSE file (this repo is public on GitHub; license choice not yet decided).

## Publishing

Packages are published to NuGet.org automatically by `.github/workflows/publish.yml` when a `v*.*.*`
tag is pushed. The workflow builds and packs all five source projects at the tagged version, then
pushes them to `https://api.nuget.org/v3/index.json`.

## Usage

### Host (inside the Xperience web app)

Add `Xperience.Relay.Kentico` and `Xperience.Relay.Hosting` as `PackageReference`s in the
Xperience web app project, then wire up DI and routing:

```csharp
// Program.cs / Startup.cs

builder.Services.AddRelayCore(typeof(MoveCommand).Assembly);   // verb registry + dispatcher
builder.Services.AddRelayKentico();                             // Kentico-backed handlers
builder.Services.AddRelayHosting();                             // API key filter

builder.Services.Configure<RelayKenticoOptions>(options =>
{
    options.ServiceAccountUserName = "relay-service";  // Kentico user attributed to changes (audit only)
    options.DefaultLanguageName    = "en";             // used when a command doesn't specify a language
    options.DefaultWorkspaceName   = "Default";        // used when a command doesn't specify a workspace
});

builder.Services.Configure<RelayHostingOptions>(options =>
{
    options.ApiKey = "your-secret-key";   // must be set -- no default
    // options.ApiKeyHeaderName = "X-Relay-Api-Key";  // default
    // options.BasePath         = "/api/relay";        // default
});

// ---

app.MapRelayEndpoints();   // exposes /api/relay/commands, /api/relay/batch, /api/relay/verbs
```

### Client (remote caller)

Add `Xperience.Relay.Client` as a `PackageReference` in the calling project (no Kentico SDK
required), then register `RelayClient` as a typed `HttpClient`:

```csharp
builder.Services.AddRelayClient(options =>
{
    // Must end with a trailing slash so relative paths resolve correctly
    options.BaseAddress      = new Uri("https://your-xperience-site.com/api/relay/");
    options.ApiKey           = "your-secret-key";
    // options.ApiKeyHeaderName = "X-Relay-Api-Key";  // default
});
```

Inject `RelayClient` and call verbs directly:

```csharp
public class MyService(RelayClient relay)
{
    public async Task RunAsync()
    {
        // Single command
        var result = await relay.ExecuteAsync(new MoveCommand
        {
            WebPageId       = 42,
            ParentWebPageId = 7,
        });

        // Typed result data
        var info = await relay.ExecuteAsync(new GetPageInfoCommand { WebPageId = 42 });
        var pageInfo = info.GetData<WebPageInfo>();

        // Batch -- commands run in order, results returned in the same order
        var batch = await relay.ExecuteBatchAsync(new IRelayCommand[]
        {
            new CreateContentHubFolderCommand { FolderPath = "Imports/Audio" },
            new CreateContentItemCommand
            {
                ContentTypeName = "Podcast.Episode",
                DisplayName     = "Episode 1",
                Asset = new RelayAsset
                {
                    FieldName = "AudioFile",
                    FileName  = "episode-1.mp3",
                    Base64    = Convert.ToBase64String(File.ReadAllBytes("episode-1.mp3")),
                },
            },
        });

        // Discovery -- lists verbs supported by the deployed relay endpoint
        var discovery = await relay.GetDiscoveryAsync();
    }
}
```
