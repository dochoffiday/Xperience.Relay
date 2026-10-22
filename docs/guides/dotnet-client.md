# Call Relay from .NET

`Xperience.Relay.Client` is a small library for calling a deployed relay. It depends on `HttpClient` and the `Xperience.Relay.Contracts` types, and nothing from Kentico, so it is safe to reference from any caller. It targets `net8.0` and `netstandard2.0`, which includes .NET Framework 4.7.2 and later.

## Install and register

```bash
dotnet add package Xperience.Relay.Client
```

Register the client in dependency injection. It is added as a typed `HttpClient`:

```csharp
using Xperience.Relay.Client;

builder.Services.AddRelayClient(options =>
{
    // The trailing slash is required so that "commands", "batch" and "verbs" resolve under it.
    options.BaseAddress = new Uri("https://your-site.example/api/relay/");
    options.ApiKey = builder.Configuration["Relay:ApiKey"]!;
    // options.ApiKeyHeaderName = "X-Relay-Api-Key";   // the default; must match the site
});
```

Keep the key out of source control: use user secrets, environment variables or your platform's secret store.

## Send a command

Inject `RelayClient`, build a command from `Xperience.Relay.Contracts.Commands`, and call `ExecuteAsync`:

```csharp
using Xperience.Relay.Client;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;

public class PageService(RelayClient relay)
{
    public async Task MoveAsync(int pageId, int newParentId)
    {
        var result = await relay.ExecuteAsync(new MoveWebPageCommand
        {
            WebPageId = pageId,
            ParentWebPageId = newParentId,
        });

        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error);
        }
    }

    public async Task<WebPageInfo?> FindAsync(int pageId)
    {
        var result = await relay.ExecuteAsync(new GetPageInfoCommand { WebPageId = pageId });
        return result.Success ? result.GetData<WebPageInfo>() : null;
    }
}
```

The client reads each command's verb from its `[RelayCommand]` attribute, so you never type verbs.

`GetData<T>()` reads `result.Data` as the type the command returns. Each command's section names the type, and the [Types](../reference/types.md) page lists them.

### Field values

Commands that take field values (`Fields` on the create and update commands) use `Dictionary<string, JsonElement>`, so the value keeps its JSON type. Build them with `JsonSerializer.SerializeToElement`:

```csharp
using System.Text.Json;

var result = await relay.ExecuteAsync(new UpdateWebPageCommand
{
    WebPageId = 68,
    Fields = new()
    {
        ["ArticleTitle"] = JsonSerializer.SerializeToElement("Cold brew at home"),
        ["ArticlePagePublishDate"] = JsonSerializer.SerializeToElement(new DateTime(2026, 10, 1)),
    },
});
```

## Errors

| Situation | What `ExecuteAsync` does |
|---|---|
| The command failed (not found, a check failed, Kentico refused) | Returns a result with `Success` false and the reason in `Error`. |
| The site answered with an HTTP error (`401`, `403`, `413`, `500`) | Returns a failed result too, with a message that holds the status and the body: `Relay endpoint returned 403: ...`. |
| The site can't be reached, or the call is cancelled | Throws, as `HttpClient` does. |

[`ExecuteBatchAsync`](batches.md) behaves differently: a non-success HTTP status throws an `HttpRequestException`.

## Batches and discovery

```csharp
var batch = await relay.ExecuteBatchAsync(
[
    new GetPageInfoCommand { WebPageId = 9 },
    new GetTaxonomiesCommand(),
]);

foreach (var result in batch.Results) { /* one per command, in order */ }

var discovery = await relay.GetDiscoveryAsync();
// discovery.RelayVersion and discovery.Verbs: what the deployed site supports
```

See [Run several commands at once](batches.md).

## Timeouts and handlers

`AddRelayClient` doesn't expose the `HttpClient` it creates. To change the timeout, add handlers (retries, logging) or share a client, register it yourself:

```csharp
builder.Services.AddHttpClient<RelayClient>(http =>
{
    http.BaseAddress = new Uri("https://your-site.example/api/relay/");
    http.DefaultRequestHeaders.Add("X-Relay-Api-Key", apiKey);
    http.Timeout = TimeSpan.FromMinutes(5);   // large uploads take a while
});
// Chain .AddHttpMessageHandler(...) here for retries or logging.
```

## .NET Framework callers

A .NET Framework 4.7.2 or later application (tested on 4.8) can install `Xperience.Relay.Client` from NuGet. A classic ASP.NET WebForms app has no DI container to call `AddRelayClient` on, but `RelayClient` only needs an `HttpClient`. Create one yourself and keep it in a `static` field, so it is created once for the lifetime of the app:

```csharp
private static readonly RelayClient Relay = CreateRelayClient();

private static RelayClient CreateRelayClient()
{
    // The trailing slash is required so that relative paths resolve correctly.
    var http = new HttpClient { BaseAddress = new Uri("https://your-site.example/api/relay/") };
    http.DefaultRequestHeaders.Add("X-Relay-Api-Key", "YOUR_API_KEY");
    return new RelayClient(http);
}
```

`RelayClient` uses `ConfigureAwait(false)` throughout, so it is safe to block on from a handler that can't be async (`.GetAwaiter().GetResult()`) without deadlocking classic ASP.NET's synchronization context. An async page (`<%@ Page Async="true" %>` with `Page.RegisterAsyncTask`) is still preferable, because it frees the request thread while it waits.
