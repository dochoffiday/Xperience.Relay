# Xperience.Relay

Drive an Xperience by Kentico site remotely, over HTTP. Relay runs **inside** your deployed Xperience web app and exposes Kentico operations as a small, secured API: create and publish pages, change fields, upload files, query content, manage taxonomies and more. You call it with a few lines of C#, or from any HTTP client.

**Documentation: <https://dochoffiday.github.io/Xperience.Relay/>**

## Why this exists

Kentico's documentation states that applications deployed to the SaaS environment can't be connected to from external .NET applications. That rules out the usual way to automate a site: a console app or library that talks to the database or the management API directly.

Relay moves the Kentico-aware code to where it can run. Its handlers live in the deployed web app, behind a thin HTTP API protected by API keys, and a lightweight client calls it remotely. Only the packages that need the Kentico SDK depend on it, so a caller never does.

## Packages

| Package | Needs the Kentico SDK? | Purpose |
|---|---|---|
| `Xperience.Relay.Contracts` | No | Commands, results and the envelope, shared by every other package. |
| `Xperience.Relay.Core` | No | Command dispatch, pipeline behaviors and the verb registry. |
| `Xperience.Relay.Kentico` | Yes (31.7.2 or later) | The handlers that call Kentico, the API key store and its admin application. Runs in the Xperience web app. |
| `Xperience.Relay.Hosting` | No | The ASP.NET Core endpoints and API key authentication. Runs in the Xperience web app. |
| `Xperience.Relay.Client` | No | `RelayClient`, a small HTTP client for callers. |

`Contracts` and `Client` target `net8.0` and `netstandard2.0`, so callers can be .NET Framework 4.7.2 applications too. `Core`, `Hosting` and `Kentico` target `net8.0`.

## Quick start

**In the Xperience web app**, add `Xperience.Relay.Kentico` and `Xperience.Relay.Hosting`, then:

```csharp
builder.Services.AddRelayCore(typeof(IRelayCommand).Assembly);
builder.Services.AddRelayKentico();
builder.Services.AddRelayHosting();

builder.Services.Configure<RelayKenticoOptions>(options =>
{
    options.ServiceAccountUserName = "relay-service";   // the Kentico user changes are attributed to
    options.DefaultLanguageName = "en";
});

// ...

app.MapRelayEndpoints();   // /api/relay/commands, /api/relay/batch and /api/relay/verbs
```

Create an API key in the admin, under **Configuration** > **Relay API keys**. It is shown once; until you create one, every request is rejected.

**From the caller**, add `Xperience.Relay.Client`:

```csharp
builder.Services.AddRelayClient(options =>
{
    options.BaseAddress = new Uri("https://your-site.example/api/relay/");   // keep the trailing slash
    options.ApiKey = "YOUR_API_KEY";
});

var result = await relay.ExecuteAsync(new MoveWebPageCommand { WebPageId = 42, ParentWebPageId = 7 });
var info = (await relay.ExecuteAsync(new GetPageInfoCommand { WebPageId = 42 })).GetData<WebPageInfo>();
```

Or send the JSON yourself:

```bash
curl -X POST "https://your-site.example/api/relay/commands" \
  -H "X-Relay-Api-Key: YOUR_API_KEY" -H "Content-Type: application/json" \
  -d '{ "verb": "get-page-info", "parameters": { "webPageId": 42 } }'
```

## Commands

41 commands in 7 areas. Each area has a page in the documentation with a section for every one of its commands:

| Area | Commands |
|---|---|
| [Web pages](https://dochoffiday.github.io/Xperience.Relay/commands/web-pages/) | create, update, move, publish, unpublish, delete, update the slug, get, query |
| [Content items](https://dochoffiday.github.io/Xperience.Relay/commands/content-items/) | create, update, move, publish, unpublish, delete, get, query, search, usage, content hub folders |
| [Assets](https://dochoffiday.github.io/Xperience.Relay/commands/assets/) | rename, re-optimize |
| [Taxonomies and tags](https://dochoffiday.github.io/Xperience.Relay/commands/taxonomies/) | create, update, move, delete, get |
| [Custom objects](https://dochoffiday.github.io/Xperience.Relay/commands/custom-objects/) | create, update, delete, query (Admin) |
| [Forms and contacts](https://dochoffiday.github.io/Xperience.Relay/commands/forms-and-contacts/) | get form submissions, delete contacts (Admin) |
| [SQL](https://dochoffiday.github.io/Xperience.Relay/commands/sql/) | guarded raw SQL (Admin) |

Every key has a tier, **Read**, **Write** or **Admin**, and each command needs a minimum tier. See [Access tiers and API keys](https://dochoffiday.github.io/Xperience.Relay/concepts/access-tiers/).

## Learn more

- [Getting started](https://dochoffiday.github.io/Xperience.Relay/getting-started/)
- [How Relay works](https://dochoffiday.github.io/Xperience.Relay/concepts/how-it-works/)
- [Create, edit and publish a page](https://dochoffiday.github.io/Xperience.Relay/guides/page-lifecycle/)
- [Configuration](https://dochoffiday.github.io/Xperience.Relay/configuration/) and [Security](https://dochoffiday.github.io/Xperience.Relay/security/)
- [Design notes](https://dochoffiday.github.io/Xperience.Relay/design-notes/), including what isn't built

## Contributing and releases

See [Contributing](https://dochoffiday.github.io/Xperience.Relay/contributing/). Packages are published to NuGet by `.github/workflows/publish.yml` when a `v*.*.*` tag is pushed, and the documentation is built and published from `master` by `.github/workflows/docs.yml`. The docs sources are in [docs/](docs/).

## Disclaimer

Xperience.Relay is an independent project. It is not created by, endorsed by or affiliated with Kentico. "Kentico" and "Xperience by Kentico" are trademarks of their respective owners.

## License

[MIT](LICENSE)
