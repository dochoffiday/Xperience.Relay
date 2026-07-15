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
| `Xperience.Relay.Kentico` | Yes (`Kentico.Xperience.Core` >= 30.12.2) | Handlers that actually call Kentico APIs (`IWebPageManager`, `IContentItemManager`, ...). Deployed as part of the live Xperience app. |
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
adding a second generic type parameter through the dispatcher would buy nothing. Query-style commands
put their DTO in `Data`; callers deserialize based on which command they called.
`RelayCommandResult.GetData<T>()` handles this: it tries a direct cast first, falls back to
`JsonElement.Deserialize<T>` if the result arrived over the wire, and round-trips through JSON as a
last resort.

## Commands

| Command | Parameters | Returns |
|---|---|---|
| `move-web-page` | `WebPageId`, `ParentWebPageId` | — |
| `move-content-item` | `ContentItemIds`, `ContentFolderId` | — |
| `get-page-info` | `WebPageId`, `LanguageName?` | `WebPageInfo` |
| `get-page` | `WebPageId`, `LanguageName?` | `WebPageData` |
| `get-content-info` | `ContentItemId` | `ContentInfo` |
| `get-content` | `ContentItemId`, `LanguageName?` | `ContentData` |
| `get-content-hub-folder` | `ContentFolderId?`, `CodeName?`, `FolderPath?`, `WorkspaceName?` | `GetContentHubFolderResult` |
| `create-content-item` | `ContentTypeName`, `DisplayName`, `LanguageName?`, `WorkspaceName?`, `ContentFolderId?`, `Fields?`, `LinkedItemFields?`, `TagFields?`, `Assets?` | `CreateContentItemResult` |
| `create-web-page` | `WebsiteChannelName?`, `ParentWebPageItemId`, `ContentTypeName`, `DisplayName`, `LanguageName?`, `UrlSlug?`, `Fields?`, `LinkedItemFields?`, `TagFields?`, `Assets?`, `PublishAfterCreate?` | `CreateWebPageResult` |
| `query-web-page-items` | `ContentTypeNames?`, `WebsiteChannelName?`, `LanguageName?`, `Columns?`, `WhereEquals?` | `QueryItemsResult` |
| `query-reusable-items` | `ContentTypeNames?`, `LanguageName?`, `Columns?`, `WhereEquals?` | `QueryItemsResult` |
| `update-web-page` | `WebPageId`, `LanguageName?`, `Fields?`, `LinkedItemFields?`, `TagFields?`, `Assets?` | — |
| `update-content-item` | `ContentItemId`, `LanguageName?`, `Fields?`, `LinkedItemFields?`, `TagFields?`, `Assets?` | — |
| `update-slug` | `WebPageId`, `LanguageName?`, `Slug` | — |
| `publish-web-page` | `WebPageId`, `LanguageName?` | — |
| `unpublish-web-page` | `WebPageId`, `LanguageName?` | — |
| `publish-content-item` | `ContentItemId`, `LanguageName?` | — |
| `unpublish-content-item` | `ContentItemId`, `LanguageName?` | — |
| `delete-web-page` | `WebPageId`, `LanguageName?`, `Permanently?`, `RedirectToWebPageId?` | — |
| `delete-content-item` | `ContentItemId`, `LanguageName?` | — |
| `reoptimize-asset` | `ContentItemId`, `FieldName`, `LanguageName?` | — |
| `rename-asset` | `ContentItemId`, `FieldName`, `AssetName`, `LanguageName?` | — |
| `query-sql` | `Query` | `QuerySqlResult` |

**Notes:**
- `LanguageName` defaults to `RelayKenticoOptions.DefaultLanguageName` when omitted.
- `WorkspaceName` defaults to `RelayKenticoOptions.DefaultWorkspaceName` when omitted.
- `get-page` / `get-content` compose their `*-info` counterpart internally and layer field data on top.
- `get-page-info` and `get-content-info` are the cheap, system-fields-only versions -- useful for resolving a path to an ID before a `move` without fetching full content fields.
- `get-content-hub-folder` accepts exactly one of `ContentFolderId` (numeric ID), `CodeName` (global code name), or `FolderPath` (slash-separated display-name path). The `FolderPath` mode is idempotent — it creates any missing path segments along the way. `WorkspaceName` is only required when using `FolderPath`.
- `create-content-item` accepts one or more binary files via `Assets` (each a Base64-encoded `RelayAsset`), publishes the item after creation, and optionally moves it into a content hub folder. `create-web-page`, `update-web-page`, and `update-content-item` accept the same `Assets` list to upload into any number of asset fields in a single call.
- `query-web-page-items` and `query-reusable-items` both include draft content (`ForPreview = true`). `ContentTypeNames` accepts zero or more content type names; an empty list queries across all types (Kentico may or may not support this — if it doesn't, it will surface as a fail result). Results are a union across all listed types. Empty `Columns` returns all columns. `WebsiteChannelName` is required on `query-web-page-items` and defaults to `RelayKenticoOptions.DefaultWebsiteChannelName`.
- `update-web-page` preserves the page's current published/draft state -- re-publishes if it was published, leaves as draft otherwise. `LinkedItemFields` maps field name to a list of content item GUIDs; `TagFields` maps field name to a list of tag GUIDs; pass an empty list for either to clear that field.
- `update-slug` updates the URL slug on a web page. Follows the same published/draft state preservation as `update-web-page` -- re-publishes if the page was published so the slug change goes live immediately.
- `update-content-item` is the reusable content item equivalent of `update-web-page` -- same field/linked-item/tag shape, same published/draft state preservation, no channel required.
- `publish-web-page` / `unpublish-web-page` and `publish-content-item` / `unpublish-content-item` are no-ops when the item is already in the target state — safe to call idempotently.
- `create-web-page` creates a new web page as an `InitialDraft`. Set `PublishAfterCreate: true` to publish immediately. `WebsiteChannelName` is matched against the channel code name (not the display name). `ParentWebPageItemId` is required (use the target parent's `WebPageItemID`).
- `delete-web-page` deletes a language variant of a web page. When `Permanently` is false the page goes to the recycle bin; set it to true to bypass the recycle bin. `RedirectToWebPageId` optionally creates a redirect to another page after deletion.
- `delete-content-item` deletes a language variant of a reusable content item. If it's the last variant the parent content item is also removed.
- `reoptimize-asset` re-triggers Kentico's asset optimization pipeline for an existing asset field without transferring any binary data over the wire. The file already lives on the server; the handler looks up its current metadata and physical path, then re-submits it through `ContentItemAssetMetadataWithSource` so Kentico processes it identically to the initial upload. Preserves the item's published/draft state.
- `rename-asset` renames an existing asset field without transferring any binary data over the wire. Same server-side lookup pattern as `reoptimize-asset` — the file is re-submitted with a fresh identifier and the new name in the metadata. `AssetName` should include the extension (e.g. `"report-2024.pdf"`). Preserves the item's published/draft state.
- `query-sql` executes a read-only SQL query against the Xperience database. Only `SELECT` and `WITH...SELECT` statements are permitted; DML/DDL keywords are rejected before execution. `QuerySqlResult` contains `Columns: string[]` and `Rows: string?[][]`. This is an application-level guard — configure a read-only DB login at the database level as the primary control.

## Usage

### Host (inside the Xperience web app)

Add `Xperience.Relay.Kentico` and `Xperience.Relay.Hosting` as `PackageReference`s in the
Xperience web app project, then wire up DI and routing:

```csharp
// Program.cs / Startup.cs

builder.Services.AddRelayCore(typeof(MoveWebPageCommand).Assembly);   // verb registry + dispatcher
builder.Services.AddRelayKentico();                             // Kentico-backed handlers
builder.Services.AddRelayHosting();                             // API key filter

builder.Services.Configure<RelayKenticoOptions>(options =>
{
    options.ServiceAccountUserName      = "relay-service";  // Kentico user attributed to changes (audit only)
    options.DefaultLanguageName         = "en";             // used when a command doesn't specify a language
    options.DefaultWorkspaceName        = "Default";        // used when a command doesn't specify a workspace
    options.DefaultWebsiteChannelName   = "MyChannel";      // required for query-web-page-items
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

Inject `RelayClient` and call commands directly:

```csharp
public class MyService(RelayClient relay)
{
    public async Task RunAsync()
    {
        // Single command
        var result = await relay.ExecuteAsync(new MoveWebPageCommand
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
            new GetContentHubFolderCommand { FolderPath = "Imports/Audio" },
            new CreateContentItemCommand
            {
                ContentTypeName = "Podcast.Episode",
                DisplayName     = "Episode 1",
                Assets =
                [
                    new RelayAsset
                    {
                        FieldName = "AudioFile",
                        FileName  = "episode-1.mp3",
                        Base64    = Convert.ToBase64String(File.ReadAllBytes("episode-1.mp3")),
                    },
                ],
            },
        });

        // Discovery -- lists commands supported by the deployed relay endpoint
        var discovery = await relay.GetDiscoveryAsync();
    }
}
```

## Non-obvious implementation decisions

**Why `MoveWebPageCommandHandler` doesn't use `IWebsiteChannelContext`.** Kentico's docs show
`IWebPageManager` instances created via `webPageManagerFactory.Create(websiteChannelContext.WebsiteChannelID, userId)`
-- but that assumes you're inside a request already scoped to a known channel (e.g. rendering a
page on that channel's site). Relay commands arrive channel-agnostic: a bare `WebPageId` with no
known channel. So `MoveWebPageCommandHandler` resolves the channel ID first via
`IInfoProvider<WebPageItemInfo>`, reading `WebPageItemWebsiteChannelID`, *then* creates the
channel-scoped manager. `GetPageInfoCommandHandler` and `UpdateWebPageCommandHandler` use the same
direct provider approach for the same reason.

**Why `LanguageName` exists on some commands but not others.** `IWebPageManager.GetWebPageMetadata`
and `IContentItemManager.GetContentItemMetadata` are language-neutral (no language parameter) --
confirmed by reflecting over the real compiled `Kentico.Xperience.Core` SDK, not by guessing. But
since the page-info handler is implemented via a content item query instead (see above), and any
`ContentItemQueryBuilder` query requires `.InLanguage(...)`, `GetPageInfoCommand`, `GetPageCommand`,
`GetContentCommand`, `UpdateWebPageCommand`, and `CreateContentItemCommand` all carry a nullable
`LanguageName` (defaulting to `RelayKenticoOptions.DefaultLanguageName`). `GetContentInfoCommand`
doesn't need it, since it goes through `IContentItemManager.GetContentItemMetadata` directly.

**Why `move-web-page` doesn't specify a sibling order.** The Kentico docs show `MoveWebPageParameters`
taking only `(webPageId, parentWebPageId)` with no order argument, implying Kentico handles
last-child placement itself. The handler passes those two arguments and lets Kentico decide
placement order.

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
they expect for the command they called.

**Why asset commands send binary files as Base64.** All relay commands travel as plain JSON
inside `RelayCommandEnvelope.Parameters`. Rather than designing a separate multipart upload path
just for asset-bearing commands, the asset bytes are Base64-encoded in `RelayAsset.Base64` and
decoded server-side to temp files. The handler writes each asset to a temp path, wraps it in
Kentico's `ContentItemAssetMetadataWithSource`, and deletes all temp files in a `finally` block
regardless of outcome. `create-content-item`, `create-web-page`, `update-content-item`, and
`update-web-page` all support an `Assets` list so multiple asset fields can be populated in a
single command.

**Why `update-web-page` reads `ContentItemCommonDataVersionStatus` before writing.** The handler
needs to know whether to re-publish after the update. It reads `VersionStatus` from the content
query result rather than calling a separate metadata API, so the channel ID and published state are
resolved in a single round-trip. If the page was published the handler calls `TryPublish` after
`TryUpdateDraft`; if it was a draft the update stays as a draft.

**Why `get-content-hub-folder` uses `IInfoProvider<ContentFolderInfo>` instead of
`IContentFolderManager.Get`.** `IContentFolderManager` creates one folder at a time under a known
parent, and there's no "ensure path" API. The handler splits the path and walks segments. The
existence check uses `IInfoProvider<ContentFolderInfo>` filtered by both
`ContentFolderParentFolderID` and `ContentFolderDisplayName` rather than `folderManager.Get(name)`,
because `Get` is a global code-name lookup -- it would silently match a same-named folder anywhere
in the tree and cause the walk to jump to the wrong branch. The `Create` call also omits `Name` so
Kentico generates a safe code name from the display name, since raw path segments may contain spaces
or special characters that aren't valid code name characters.

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

## Publishing

Packages are published to NuGet.org automatically by `.github/workflows/publish.yml` when a `v*.*.*`
tag is pushed. The workflow builds and packs all five source projects at the tagged version, then
pushes them to `https://api.nuget.org/v3/index.json`.
