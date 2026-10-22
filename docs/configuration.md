# Configuration

Relay is configured with ordinary .NET options, set in `Program.cs` with `services.Configure<T>(...)`. API keys are not part of it: they are managed in the Xperience admin (see [Access tiers and API keys](concepts/access-tiers.md)).

```csharp
builder.Services.Configure<RelayKenticoOptions>(options =>
{
    options.ServiceAccountUserName = "relay-service";
    options.DefaultLanguageName = "en";
});

builder.Services.Configure<RelayHostingOptions>(options =>
{
    options.BasePath = "/api/relay";
});
```

To keep the values in `appsettings.json` instead, bind a section:

```csharp
builder.Services.Configure<RelayKenticoOptions>(builder.Configuration.GetSection("Relay:Kentico"));
```

```json
{
  "Relay": {
    "Kentico": {
      "ServiceAccountUserName": "relay-service",
      "DefaultLanguageName": "en",
      "DefaultWebsiteChannelName": "MyWebsite"
    }
  }
}
```

## Kentico options

`RelayKenticoOptions`, in `Xperience.Relay.Kentico`. They apply on the site, to the handlers.

### ServiceAccountUserName

| | |
|---|---|
| Default | empty |
| Needed by | Every command that creates, changes, moves, publishes or deletes pages and content items, and [`get-content-info`](commands/content-items/index.md#get-content-info) and [`get-content`](commands/content-items/index.md#get-content) |

The user name of the Kentico user that Relay's changes are attributed to: Kentico shows it as who modified something. It has to be an existing user. Its permissions are **not** checked: Kentico's management API uses the user for attribution only, so a key's tier, not this user, decides what a caller may do. A dedicated user, such as `relay-service`, makes Relay's changes easy to recognize.

Commands that need it fail with `RelayKenticoOptions.ServiceAccountUserName must be configured before relay commands that modify content can run.` when it isn't set, and with `Relay service account user '...' was not found.` when no such user exists. The taxonomy, tag, custom object, form, contact and SQL commands, `get-page`, `get-page-info` and the queries don't use it.

### DefaultLanguageName

| | |
|---|---|
| Default | `en` |

The language used by commands that have a `languageName` parameter when the call leaves it out. It is the code name of a content language, as in the admin. See [Defaults and languages](concepts/defaults-and-languages.md).

### DefaultWorkspaceName

| | |
|---|---|
| Default | empty |

The workspace used by `create-content-item` and by `get-content-hub-folder` (with a `folderPath`) when the call has no `workspaceName`. It is a workspace code name. Leave it empty to make callers name the workspace every time.

### DefaultWebsiteChannelName

| | |
|---|---|
| Default | empty |

The website channel used by `create-web-page`, `query-web-page-items` and `search-content` (for page types) when the call has no `websiteChannelName`. It is a channel code name, not its display name. Leave it empty to make callers name the channel every time.

### SqlQueryTimeoutSeconds

| | |
|---|---|
| Default | `30` |

How long `query-sql` and `execute-sql` may run before they are cut off, in seconds.

## Hosting options

`RelayHostingOptions`, in `Xperience.Relay.Hosting`. They apply to the HTTP endpoints. They are read when the endpoints are mapped, so set them before `app.MapRelayEndpoints()` runs, which they are if you configure them on the service collection as above.

### BasePath

| | |
|---|---|
| Default | `/api/relay` |

The route the endpoints are mapped under: `{BasePath}/commands`, `{BasePath}/batch` and `{BasePath}/verbs`. Choose one that doesn't clash with your own routes or with Kentico's page routing.

### ApiKeyHeaderName

| | |
|---|---|
| Default | `X-Relay-Api-Key` |

The header callers send their key in. Callers have to be configured with the same name.

### MaxRequestBodySizeBytes

| | |
|---|---|
| Default | `268435456` (256 MB) |

The largest request body the relay accepts, in bytes. A larger body is rejected with `413`. Set it to `null` for no limit. The default makes room for [Base64-encoded file uploads](guides/uploading-assets.md), which are a third bigger than the file. Infrastructure in front of the site, such as a reverse proxy or the hosting platform, may enforce its own limit.

## Client options

`RelayClientOptions`, in `Xperience.Relay.Client`. They apply to the caller, and are set in `AddRelayClient(options => ...)`.

### BaseAddress

| | |
|---|---|
| Default | none: required |

The address the relay's endpoints are under, for example `https://your-site.example/api/relay/`. It **must end with a slash**: the client resolves `commands`, `batch` and `verbs` relative to it, and without the slash the last path segment would be replaced instead of added to.

### ApiKey

| | |
|---|---|
| Default | empty |

The API key to send. Keep it out of source control.

### ApiKeyHeaderName

| | |
|---|---|
| Default | `X-Relay-Api-Key` |

The header to send the key in. It must match the site's [`ApiKeyHeaderName`](#apikeyheadername).
