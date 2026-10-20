# Getting started

This page takes you from nothing to a working call: install Relay in your Xperience web app, create an API key, and send a command.

## What you need

- An Xperience by Kentico site on version **31.7.2 or later**, running on .NET 8. The `Xperience.Relay.Kentico` package depends on `Kentico.Xperience.Core` and `Kentico.Xperience.Admin` 31.7.2.
- A Kentico user to attribute Relay's changes to (see [step 3](#3-configure-relay)).
- Somewhere to call from: any HTTP client, or a .NET project.

## 1. Install the packages

Add two packages to the Xperience web app project. The others come with them.

```bash
dotnet add package Xperience.Relay.Kentico
dotnet add package Xperience.Relay.Hosting
```

## 2. Register Relay

In `Program.cs`, register the services and map the endpoints:

```csharp
using Xperience.Relay.Contracts;
using Xperience.Relay.Core;
using Xperience.Relay.Hosting;
using Xperience.Relay.Kentico;

builder.Services.AddRelayCore(typeof(IRelayCommand).Assembly);   // verb registry and dispatcher
builder.Services.AddRelayKentico();                             // the handlers, and the API key store
builder.Services.AddRelayHosting();                             // API key authentication

// ...

app.MapRelayEndpoints();   // /api/relay/commands, /api/relay/batch and /api/relay/verbs
```

`MapRelayEndpoints` works anywhere you map routes. The endpoints sit under `/api/relay` unless you change [`BasePath`](configuration.md#basepath).

The first time the site starts, Relay creates its `Relay_ApiKey` table in the database. There is nothing to run by hand.

## 3. Configure Relay

Relay needs to know which Kentico user to attribute its changes to, and it can fill in defaults for parameters you leave out:

```csharp
builder.Services.Configure<RelayKenticoOptions>(options =>
{
    options.ServiceAccountUserName = "relay-service";   // a Kentico user: "modified by" shows this user
    options.DefaultLanguageName = "en";                 // used when a command has no languageName
    options.DefaultWorkspaceName = "MyWorkspace";       // used when a command has no workspaceName
    options.DefaultWebsiteChannelName = "MyWebsite";    // used when a command has no websiteChannelName
});
```

Create the service account in the administration under **Users**. Its permissions are not checked, so it only serves to show who made a change; a dedicated user makes Relay's changes easy to recognize. Every setting is described on the [Configuration](configuration.md) page.

## 4. Create an API key

Until you create a key, every request is rejected with `401`.

1. Open the administration and go to **Configuration** > **Relay API keys**.
2. Choose **New API key**, give it a name (who or what will use it) and pick an [access tier](concepts/access-tiers.md): Read, Write or Admin.
3. **Copy the key from the message that appears.** It starts with `rly_`, and it is shown once: only a hash of it is stored.

If you lose a key, delete it and create a new one. Deleting a key revokes it from the very next request.

## 5. Make a call

Deploy the site, then ask the relay which commands it supports. Any valid key can do this:

```bash
curl "https://your-site.example/api/relay/verbs" -H "X-Relay-Api-Key: YOUR_API_KEY"
```

Then run a command. This one looks up a page:

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{ "verb": "get-page-info", "parameters": { "webPageId": 9 } }'
    ```

=== "PowerShell"

    ```powershell
    $body = '{ "verb": "get-page-info", "parameters": { "webPageId": 9 } }'
    Invoke-RestMethod "https://your-site.example/api/relay/commands" -Method Post `
        -Headers @{ "X-Relay-Api-Key" = "YOUR_API_KEY" } -ContentType "application/json" -Body $body
    ```

A `success` of `true` means it worked. Anything else comes with an `error`; see [Errors](reference/errors.md) for what the common ones mean.

## 6. Call it from .NET

Add `Xperience.Relay.Client` to the calling project. It has no dependency on Kentico.

```csharp
builder.Services.AddRelayClient(options =>
{
    options.BaseAddress = new Uri("https://your-site.example/api/relay/");   // keep the trailing slash
    options.ApiKey = "YOUR_API_KEY";
});
```

```csharp
public class PageReader(RelayClient relay)
{
    public async Task<WebPageInfo?> FindAsync(int webPageId)
    {
        var result = await relay.ExecuteAsync(new GetPageInfoCommand { WebPageId = webPageId });
        return result.Success ? result.GetData<WebPageInfo>() : null;
    }
}
```

[Call Relay from .NET](guides/dotnet-client.md) covers batches, errors and .NET Framework callers.

## If something goes wrong

| You see | It usually means |
|---|---|
| HTTP `401`, no body | The key is missing, mistyped or deleted. Check the header name (`X-Relay-Api-Key` unless you changed it). |
| HTTP `403` | The key's tier is lower than the command needs. The body names both. |
| "ServiceAccountUserName must be configured" | Set [`ServiceAccountUserName`](configuration.md#serviceaccountusername) to an existing Kentico user. |
| "WebsiteChannelName is required" or "WorkspaceName is required" | Pass the parameter, or set the matching default in the options. |

## Next

- [How Relay works](concepts/how-it-works.md)
- [Access tiers and API keys](concepts/access-tiers.md)
- [Create, edit and publish a page](guides/page-lifecycle.md)
- [Commands](commands/index.md)
