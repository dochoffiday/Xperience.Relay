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
| `Xperience.Relay.Kentico` | Yes (`Kentico.Xperience.Core` and `Kentico.Xperience.Admin` >= 31.7.2) | Handlers that actually call Kentico APIs (`IWebPageManager`, `IContentItemManager`, ...), plus the API key store and its admin application. Deployed as part of the live Xperience app. |
| `Xperience.Relay.Hosting` | No | ASP.NET Core endpoints (`/commands`, `/batch`, `/verbs`) exposing the dispatcher over HTTP, with API-key auth. |
| `Xperience.Relay.Client` | No | Lightweight remote caller (`RelayClient`) -- no Kentico SDK dependency, just HTTP + the `Contracts` types. Resolves each command's verb from its `[RelayCommand]` attribute and posts to `/commands`/`/batch`, or reads `/verbs`. |

`Xperience.Relay.Contracts` and `Xperience.Relay.Client` multi-target `net8.0` and `netstandard2.0`, so
remote callers can also be .NET Framework 4.7.2+ apps (e.g. classic ASP.NET WebForms) -- see
[.NET Framework callers](#net-framework-callers-eg-classic-aspnet-webforms). `Core`, `Hosting`, and
`Kentico` are `net8.0` only.

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

| Command | Access | Parameters | Returns |
|---|---|---|---|
| `move-web-page` | Write | `WebPageId`, `ParentWebPageId` | — |
| `move-content-item` | Write | `ContentItemIds`, `ContentFolderId` | — |
| `get-page-info` | Read | `WebPageId`, `LanguageName?` | `WebPageInfo` |
| `get-page` | Read | `WebPageId`, `LanguageName?` | `WebPageData` |
| `get-content-info` | Read | `ContentItemId` | `ContentInfo` |
| `get-content` | Read | `ContentItemId`, `LanguageName?` | `ContentData` |
| `get-content-hub-folder` | Write | `ContentFolderId?`, `CodeName?`, `FolderPath?`, `WorkspaceName?` | `GetContentHubFolderResult` |
| `create-content-item` | Write | `ContentTypeName`, `DisplayName`, `LanguageName?`, `WorkspaceName?`, `ContentFolderId?`, `Fields?`, `LinkedItemFields?`, `TagFields?`, `Assets?` | `CreateContentItemResult` |
| `create-web-page` | Write | `WebsiteChannelName?`, `ParentWebPageItemId`, `ContentTypeName`, `DisplayName`, `LanguageName?`, `UrlSlug?`, `Fields?`, `LinkedItemFields?`, `TagFields?`, `Assets?`, `PublishAfterCreate?` | `CreateWebPageResult` |
| `query-web-page-items` | Read | `ContentTypeNames?`, `WebsiteChannelName?`, `LanguageName?`, `Columns?`, `WhereEquals?`, `IncludeUrls?`, `IncludeStatus?`, `PublishedOnly?` | `QueryItemsResult` |
| `query-reusable-items` | Read | `ContentTypeNames?`, `LanguageName?`, `Columns?`, `WhereEquals?`, `PublishedOnly?` | `QueryItemsResult` |
| `update-web-page` | Write | `WebPageId`, `LanguageName?`, `Fields?`, `LinkedItemFields?`, `TagFields?`, `Assets?` | — |
| `update-content-item` | Write | `ContentItemId`, `LanguageName?`, `Fields?`, `LinkedItemFields?`, `TagFields?`, `Assets?` | — |
| `update-slug` | Write | `WebPageId`, `LanguageName?`, `Slug` | — |
| `publish-web-page` | Write | `WebPageId`, `LanguageName?` | — |
| `unpublish-web-page` | Write | `WebPageId`, `LanguageName?` | — |
| `publish-content-item` | Write | `ContentItemId`, `LanguageName?` | — |
| `unpublish-content-item` | Write | `ContentItemId`, `LanguageName?` | — |
| `delete-web-page` | Write | `WebPageId`, `LanguageName?`, `Permanently?`, `RedirectToWebPageId?` | — |
| `delete-content-item` | Write | `ContentItemId`, `LanguageName?` | — |
| `reoptimize-asset` | Write | `ContentItemId`, `FieldName`, `LanguageName?` | — |
| `rename-asset` | Write | `ContentItemId`, `FieldName`, `AssetName`, `LanguageName?` | — |
| `get-taxonomies` | Read | — | `GetTaxonomiesResult` |
| `create-taxonomy` | Write | `Title`, `Description?`, `CodeName?` | `CreateTaxonomyResult` |
| `update-taxonomy` | Write | `TaxonomyId`, `Title?`, `Description?` | — |
| `delete-taxonomy` | Write | `TaxonomyId` | — |
| `get-tags` | Read | `TaxonomyId?`, `TaxonomyName?`, `TagId?`, `TagName?` | `GetTagsResult` |
| `create-tag` | Write | `TaxonomyId?`, `TaxonomyName?`, `Title`, `Description?`, `CodeName?`, `ParentTagId?`, `Order?` | `CreateTagResult` |
| `update-tag` | Write | `TagId`, `Title?`, `Description?` | — |
| `move-tag` | Write | `TagId`, `TargetParentTagId`, `Order` | — |
| `delete-tag` | Write | `TagId` | — |
| `search-content` | Read | `ContentTypeName`, `Filter`, `LanguageName?`, `WebsiteChannelName?` | `SearchContentResult` |
| `query-custom-objects` | Admin | `ObjectTypeName`, `Columns?`, `WhereEquals?` | `QueryCustomObjectsResult` |
| `create-custom-object` | Admin | `ObjectTypeName`, `Fields` | `CreateCustomObjectResult` |
| `update-custom-object` | Admin | `ObjectTypeName`, `Id`, `Fields` | — |
| `delete-custom-object` | Admin | `ObjectTypeName`, `Id` | — |
| `get-content-item-usage` | Read | `ContentItemId`, `LanguageName` | `GetContentItemUsageResult` |
| `delete-contacts` | Admin | `WhereClause`, `BatchSize?` | — |
| `get-form-submissions` | Read | `FormClassName`, `Top?`, `WhereClause?`, `OrderBy?` | `GetFormSubmissionsResult` |
| `query-sql` | Admin | `Query` | `QuerySqlResult` |
| `execute-sql` | Admin | `Query`, `Confirm` | `ExecuteSqlResult` |

**Access** is the lowest API key tier that may run the verb: Read, Write or Admin, each including the tiers below it (see [API keys and access tiers](#api-keys-and-access-tiers)).

**Notes:**
- `LanguageName` defaults to `RelayKenticoOptions.DefaultLanguageName` when omitted.
- `WorkspaceName` defaults to `RelayKenticoOptions.DefaultWorkspaceName` when omitted.
- `get-page` / `get-content` compose their `*-info` counterpart internally and layer field data on top.
- `get-page-info` and `get-content-info` are the cheap, system-fields-only versions -- useful for resolving a path to an ID before a `move` without fetching full content fields.
- `get-content-hub-folder` accepts exactly one of `ContentFolderId` (numeric ID), `CodeName` (global code name), or `FolderPath` (slash-separated display-name path). The `FolderPath` mode is idempotent — it creates any missing path segments along the way. `WorkspaceName` is only required when using `FolderPath`.
- `create-content-item` accepts one or more binary files via `Assets` (each a Base64-encoded `RelayAsset`), publishes the item after creation, and optionally moves it into a content hub folder. `create-web-page`, `update-web-page`, and `update-content-item` accept the same `Assets` list to upload into any number of asset fields in a single call.
- `query-web-page-items` and `query-reusable-items` return each item's latest version, drafts included, unless `PublishedOnly: true` is set: then they return the published version of each item (field values included) and leave out items that have none. To get every page *and* know which are published, use `IncludeStatus: true` on `query-web-page-items` (below) instead. For reusable items, run the query twice -- once as usual and once with `PublishedOnly` -- and compare `ContentItemID`s; an item is published if it appears in the second result. All of these are Read-tier, so a Read key can answer "is this published?" without `query-sql`. `ContentTypeNames` accepts zero or more content type names; an empty list queries across all types (Kentico may or may not support this — if it doesn't, it will surface as a fail result). Results are a union across all listed types. Empty `Columns` returns all columns. A `WhereEquals` column must also be listed in `Columns` (the filter runs over the selected columns, so filtering on an unselected column fails with "Invalid column name"). Language fallbacks apply in both modes. `WebsiteChannelName` is required on `query-web-page-items` and defaults to `RelayKenticoOptions.DefaultWebsiteChannelName`.
- `query-web-page-items` accepts `IncludeUrls: true` to add `WebPageRelativeUrl` and `WebPageAbsoluteUrl` to every item, whether or not they're in `Columns`. The URL data is fetched by the same query, so there are no per-page lookups. `WebPageRelativeUrl` uses Kentico's app-relative `~/path` form; `WebPageAbsoluteUrl` uses the channel's domain. Both are formatted the way the site serves them (lower-case, for example), so they can differ in case from `WebPageUrlPath`. Both are empty strings for a page with no URL path. This follows Kentico's content item query default, so a page served through language fallback gets its fallback language's URL. `WebPageItemTreePath` is the content tree path, not the URL -- vanity URLs and language prefixes make them differ. By default the URL is the latest one, which can be a URL change that hasn't been published yet (a live page with unpublished changes shows its new URL); with `PublishedOnly` it is the live URL. To get the raw URL path (no `~/`), also list `WebPageUrlPath` in `Columns` (it follows the same rule).
- `query-web-page-items` accepts `IncludeStatus: true` to add two fields to every page, whether or not they're in `Columns`; the query still returns all pages, drafts included. `WebPageStatus` is the status of the page's latest version: `InitialDraft`, `Draft`, `Published` or `Unpublished`. `WebPageIsPublished` (true/false) is whether a published version exists, which is not the same thing: it is true for `Published` and for `Draft` -- a change on top of a published version, so a live page with unpublished changes has status `Draft` and is still published -- and false for `InitialDraft` (never published) and `Unpublished`. Both come from the same query, so `IncludeStatus` costs nothing extra. Both fields describe the language variant the row comes from: the requested language, or its fallback language for a page that has no variant in the requested language. A page whose variant in the requested language is unpublished is reported as unpublished, even if its fallback language's variant is published.
- `update-web-page` preserves the page's current published/draft state -- re-publishes if it was published, leaves it as a draft otherwise (see the edit verbs note below). `LinkedItemFields` maps field name to a list of content item GUIDs; `TagFields` maps field name to a list of tag GUIDs; pass an empty list for either to clear that field.
- `update-slug` updates the URL slug on a web page. Follows the same published/draft state preservation as `update-web-page` -- re-publishes if the page was published so the slug change goes live immediately.
- `update-content-item` is the reusable content item equivalent of `update-web-page` -- same field/linked-item/tag shape, same published/draft state preservation, no channel required.
- The edit verbs (`update-web-page`, `update-content-item`, `update-slug`, `reoptimize-asset`, `rename-asset`) share one flow and report what Kentico actually did. The item's state is read for the requested language only, and a language variant that doesn't exist fails with "was not found in that language". A published or unpublished item gets a draft to edit; one that already has a draft is edited in place. Only an item that was live is published again afterwards, so editing an unpublished item or a draft never publishes it. If Kentico won't create the draft, won't save the changes, or won't publish them, the result is a failure that says which step it was. When the final publish fails, the changes are saved as a draft and the live version is unchanged -- never a false success. A failed edit leaves nothing behind: the draft made for it is discarded (a draft that was already there is kept as it was), and Kentico's cached copy of the item is dropped, so the next edit doesn't write the refused value again. When an edit is saved but not live, the answer says so: "Saved as a draft; the published version is unchanged." for a live item that had pending changes, "Saved as a draft; it is not published." otherwise.
- `create-web-page`, `create-content-item`, `update-web-page` and `update-content-item` check their fields against the content type (including the fields of its reusable field schemas) before anything is created or changed, and fail with every problem found, so a bad call changes nothing. A field the type doesn't have is refused (Kentico would otherwise drop it from an update without saying so); field names aren't case-sensitive. A plain value in `Fields` must fit its field: a whole number for an integer field, a date that parses for a date field, `true`/`false` for a boolean, text no longer than the field allows, a decimal with no more digits than its column holds, and not `null` for a field that must have a value. `LinkedItemFields` only go into content item reference fields, `TagFields` into tag (taxonomy) fields and `Assets` into asset fields (Kentico would otherwise store them as JSON text in whatever field it was told to). Values for reference, asset and tag fields sent through `Fields` are not checked.
- `publish-web-page` / `unpublish-web-page` and `publish-content-item` / `unpublish-content-item` act on one language variant and report what Kentico actually did. Asking for a state the item is already in succeeds without changing anything, so they're safe to repeat. `publish-*` publishes a draft, and for an *unpublished* item it creates the draft first, so an unpublished item can be published again. `unpublish-*` unpublishes a published item; on an item that is already unpublished, or only has a draft, it does nothing. Kentico won't unpublish a published item while it has a draft with unpublished changes, so that case fails with an explanation (publish or discard the draft first). If Kentico declines an operation, or the item or language variant isn't found, the result is a failure -- never a false "Published". The content item verbs only accept reusable content items; use the page verbs for pages.
- `create-web-page` creates a new web page as an `InitialDraft`. Set `PublishAfterCreate: true` to publish immediately; if Kentico creates the page but declines to publish it, the result is a failure that includes the new page's ID, so a retry doesn't create a second page. `WebsiteChannelName` is matched against the channel code name (not the display name). `ParentWebPageItemId` is required (use the target parent's `WebPageItemID`).
- `delete-web-page` deletes a language variant of a web page. When `Permanently` is false the page goes to the recycle bin; set it to true to bypass the recycle bin. `RedirectToWebPageId` optionally creates a redirect to another page after deletion.
- `delete-content-item` deletes a language variant of a reusable content item. If it's the last variant the parent content item is also removed.
- `reoptimize-asset` re-triggers Kentico's asset optimization pipeline for an existing asset field without transferring any binary data over the wire. The file already lives on the server; the handler looks up its current metadata and physical path, then re-submits it through `ContentItemAssetMetadataWithSource` so Kentico processes it identically to the initial upload. Preserves the item's published/draft state.
- `rename-asset` renames an existing asset field without transferring any binary data over the wire.
- `get-taxonomies` returns all taxonomies with their ID, GUID, code name, title, and description.
- `create-taxonomy` / `update-taxonomy` / `delete-taxonomy` manage taxonomy records. `update-taxonomy` only allows changing `Title` and `Description`. `delete-taxonomy` cascades — all child tags are deleted. `CodeName` is auto-generated from `Title` when omitted.
- `get-tags` requires at least one filter. `TaxonomyId`/`TaxonomyName` returns all tags in a taxonomy; `TagId`/`TagName` returns the specific matching tag(s). Filters are applied in priority order: `TagId` → `TagName` → `TaxonomyId` → `TaxonomyName`.
- `create-tag` / `update-tag` / `delete-tag` manage tag records. `update-tag` only allows changing `Title` and `Description`. `delete-tag` cascades — all child tags are deleted. `CodeName` is auto-generated from `Title` when omitted. `ParentTagId` of 0 or omitted creates a root-level tag.
- `move-tag` moves a tag to a new parent and position. Set `TargetParentTagId` to 0 to move to the taxonomy root. Same server-side lookup pattern as `reoptimize-asset` — the file is re-submitted with a fresh identifier and the new name in the metadata. `AssetName` should include the extension (e.g. `"report-2024.pdf"`). Preserves the item's published/draft state.
- `query-custom-objects` / `create-custom-object` / `update-custom-object` / `delete-custom-object` provide generic CRUD for any Kentico custom module object. `ObjectTypeName` is the registered object type string (e.g. `"custom.redirect"`). `query-custom-objects` returns `QueryCustomObjectsResult { Items: List<Dictionary<string,string?>> }` where each item maps column name to its string value; omit `Columns` to get all columns. `create-custom-object` inserts a new object with the given `Fields` and returns `CreateCustomObjectResult { Id }`. `update-custom-object` patches only the supplied `Fields` on the existing object. Mutations go through Kentico's `BaseInfo.Insert/Update/Delete` so events and cache invalidation fire normally.
- `search-content` searches all published items of a single content type for a case-insensitive text match across every `Text`, `LongText`, and `RichTextHTML` field. `ContentTypeName` and `Filter` are both required. For web page content types, `WebsiteChannelName` is also required (falls back to `DefaultWebsiteChannelName`). Results in `SearchContentResult` include `ContentItemId`, `ContentItemName`, `ContentTypeName`, `Location` (tree path, web pages only), and `MatchedFields` (field name → matched value). The search is done in-process after fetching published items — avoid calling it on very large content types.
- `get-content-item-usage` wraps `IContentItemUsageRetriever.Retrieve` to find all content items that directly reference a given content item language variant. `ContentItemId` and `LanguageName` are both required. Returns `GetContentItemUsageResult { Items: List<ContentItemUsageEntry> }` where each entry exposes `ContentItemId`, `LanguageName`, `DisplayName`, `LatestVersionStatus`, `CreatedWhen`, `ModifiedWhen`, `HasImageAsset`, `ScheduledPublishWhen`, and `ScheduledUnpublishWhen`.
- `get-form-submissions` retrieves submissions for a Kentico BizForm. `FormClassName` is the form's class name (e.g. `"BF_ContactForm"`). Returns `GetFormSubmissionsResult { FormClassName, Columns: string[], Rows: string?[][] }` — the column set is determined by the first row and will vary per form. `WhereClause` is a raw SQL WHERE expression (without the `WHERE` keyword) using the form's table columns. `OrderBy` defaults to `FormInserted DESC`. The class name appears in Kentico under **Forms → Form name → Form properties → Class name**.
- `delete-contacts` bulk-deletes contacts via `IContactsBulkDeletionService.BulkDelete`. `WhereClause` is a raw SQL WHERE expression (without the `WHERE` keyword) using `ContactInfo` column names — e.g. `"ContactEmail IS NULL"` or `"ContactCreated < '2024-01-01' AND ContactEmail IS NULL"`. When `BatchSize` is omitted or 0, the service auto-loops through all matching contacts in batches of 1000. When `BatchSize` is provided, only a single batch of that size is deleted per call. **This operation is irreversible** — deletion cascades to consent agreements, activities, and email recipients.
- `query-sql` executes a read-only SQL query. Only `SELECT` and `DECLARE` statements are permitted; validation uses the ScriptDom AST parser so keyword-in-string and comment bypasses are not possible. Dangerous functions (`XP_CMDSHELL`, `OPENROWSET`, `OPENDATASOURCE`, etc.) are blocked in both modes. `QuerySqlResult` contains `Columns: string[]` and `Rows: string?[][]`. This is an application-level guard — configure a read-only DB login at the database level as the primary control.
- `execute-sql` is a two-step executor that allows `INSERT`, `UPDATE`, `DELETE`, `CREATE`, `ALTER`, `DROP`, and `EXECUTE` in addition to `SELECT`. Without `Confirm: true` the handler validates the SQL and returns `ExecuteSqlResult { Confirmed: false, StatementSummaries: string[] }` describing each statement and the tables it touches — so the caller can review before committing. With `Confirm: true` the query runs and `ExecuteSqlResult { Confirmed: true, StatementSummaries, Columns?, Rows? }` is returned; `Columns`/`Rows` are populated when execution returns a result set (SELECT or OUTPUT clause), otherwise the result confirms success with no row data. **This operation is irreversible for DML/DDL — always preview first.**

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

// Optional -- both shown with their defaults. API keys aren't configured here; see below.
builder.Services.Configure<RelayHostingOptions>(options =>
{
    // options.ApiKeyHeaderName = "X-Relay-Api-Key";
    // options.BasePath         = "/api/relay";
});

// ---

app.MapRelayEndpoints();   // exposes /api/relay/commands, /api/relay/batch, /api/relay/verbs
```

### API keys and access tiers

API keys are managed in the Xperience admin, in the **Relay API keys** application (Configuration
category) -- there's nothing to put in `appsettings.json`. The package creates its `Relay_ApiKey`
table on startup, so there's no setup beyond installing it. Until you create a key, every request is
rejected with `401`.

Create a key, give it a name and a tier, and **copy it when it's shown**: only a hash is stored, so it
can't be shown again (if it's lost, delete it and create a new one). Deleting a key revokes it on the
very next request. The listing shows each key's name, the start of the key (to tell keys apart), its
tier, when it was created, and when it was last used (updated at most every five minutes). Who can
manage keys is controlled by the application's View / Create / Delete permissions.

Each key has one tier, and each tier includes the ones below it. A request is rejected with `403` if
the key's tier is lower than the verb needs, and a `/batch` is checked as a whole before any command
in it runs.

| Tier | Verbs |
|---|---|
| **Read** | `get-page-info`, `get-page`, `get-content-info`, `get-content`, `query-web-page-items`, `query-reusable-items`, `search-content`, `get-taxonomies`, `get-tags`, `get-content-item-usage`, `get-form-submissions` |
| **Write** | everything in Read, plus `move-web-page`, `move-content-item`, `get-content-hub-folder`, `create-content-item`, `create-web-page`, `update-web-page`, `update-content-item`, `update-slug`, `publish-web-page`, `unpublish-web-page`, `publish-content-item`, `unpublish-content-item`, `delete-web-page`, `delete-content-item`, `reoptimize-asset`, `rename-asset`, `create-taxonomy`, `update-taxonomy`, `delete-taxonomy`, `create-tag`, `update-tag`, `move-tag`, `delete-tag` |
| **Admin** | everything in Write, plus `query-sql`, `execute-sql`, `query-custom-objects`, `create-custom-object`, `update-custom-object`, `delete-custom-object`, `delete-contacts` |

A command's tier is set on its `[RelayCommand("verb", Access = RelayAccess.X)]` attribute. A command
with no `Access` defaults to Admin, so a new command is never open to lower tiers by accident.

The key table and its `relay.apikey` object type can't be reached through `query-sql`, `execute-sql`, or
the custom-object commands. That's a guardrail, not a security boundary: Admin keys are fully trusted,
and an Admin key can do anything the database login can.

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

#### .NET Framework callers (e.g. classic ASP.NET WebForms)

A .NET Framework 4.7.2+ app (tested on 4.8) can install `Xperience.Relay.Client` from NuGet. There's
no DI container to call `AddRelayClient` on in a WebForms app, but `RelayClient` only needs an
`HttpClient`, so build one yourself and keep it in a `static` field so it's created once for the
lifetime of the app:

```csharp
private static readonly RelayClient Relay = CreateRelayClient();

private static RelayClient CreateRelayClient()
{
    // Trailing slash is required so relative paths resolve correctly
    var http = new HttpClient { BaseAddress = new Uri("https://your-xperience-site.com/api/relay/") };
    http.DefaultRequestHeaders.Add("X-Relay-Api-Key", "your-secret-key");
    return new RelayClient(http);
}
```

`RelayClient` uses `ConfigureAwait(false)` throughout, so it's safe to block on from a non-async
handler (`.GetAwaiter().GetResult()`) without deadlocking classic ASP.NET's synchronization context.
An async page (`<%@ Page Async="true" %>` with `Page.RegisterAsyncTask`) is still preferable, since it
frees the request thread while waiting on the relay.

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
in `Kentico.Xperience.Core` 31.6.3 (confirmed by reflecting over every type containing "Workspace" in
the compiled assemblies -- none exist in the public API at this SDK version). If a later SDK version
adds one, `GetContentInfoCommandHandler.FetchContentInfoAsync` is the place to use it.

**Why API keys are stored as a plain, unsalted SHA-256 hash.** Keys are 256 bits of randomness, so
there is nothing to brute-force and no reason for a salt or a slow password hash. A request hashes the
presented key and looks it up in an in-memory copy of the key table, which Kentico's cache invalidation
refreshes when a key is added or deleted -- so revocation is immediate and requests never hit the
database. "Last used" is written straight to the table (throttled to once per five minutes per key)
instead of through the info object, because updating the object would invalidate that cache on every
request.

**Why `get-content-hub-folder` is a Write verb.** In `FolderPath` mode it creates any missing
folders, so it can't be treated as a read.

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
  `Kentico.Xperience.Core` 31.6.3 assembly (via a throwaway reflection probe used while building this,
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
