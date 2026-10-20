# Xperience.Relay

Drive an Xperience by Kentico site remotely. Relay is a set of packages that runs **inside** your deployed Xperience web app and exposes Kentico operations over a small, secured HTTP API. You call it from a few lines of C#, or from any HTTP client: create and publish pages, change fields, upload files, query content, manage taxonomies and more.

It has 41 commands, each with its own page, and a client library that needs no Kentico SDK.

## Why Relay exists

Kentico's documentation states that applications deployed to the SaaS environment can't be connected to from external .NET applications. That rules out the usual way of automating a site: a console app or library that talks to the database or the management API directly.

So Relay moves the Kentico-aware code to where it can run. Its handlers live in the deployed web app, behind a thin HTTP API protected by API keys. The caller only needs HTTP, so a reporting tool, a migration script or a legacy WebForms app can all drive the site without a reference to Kentico.

## What you can do

<div class="grid cards" markdown>

- :material-file-document-edit: **[Web pages](commands/web-pages/index.md)**

    Create, edit, move, publish and delete pages. Find them by type, field value or status.

- :material-folder-multiple: **[Content items](commands/content-items/index.md)**

    Reusable content, content hub folders, usage and text search.

- :material-image: **[Assets](commands/assets/index.md)**

    Rename or re-optimize files in asset fields without sending them again.

- :material-tag-multiple: **[Taxonomies and tags](commands/taxonomies/index.md)**

    Manage taxonomies and nested tags.

- :material-database-cog: **[Custom objects](commands/custom-objects/index.md)**

    Generic access to any registered Kentico object type.

- :material-form-select: **[Forms and contacts](commands/forms-and-contacts/index.md)**

    Read form submissions and delete contacts in bulk.

- :material-database-search: **[SQL](commands/sql/index.md)**

    Guarded raw SQL for what the typed commands can't do.

- :material-key: **[Access tiers](concepts/access-tiers.md)**

    Read, Write and Admin API keys, managed in the Xperience admin.

</div>

## A first call

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{ "verb": "get-page-info", "parameters": { "webPageId": 9 } }'
    ```

=== "C#"

    ```csharp
    var result = await relay.ExecuteAsync(new GetPageInfoCommand { WebPageId = 9 });
    var page = result.GetData<WebPageInfo>();
    ```

The answer is always the same shape: `success`, a `message` or an `error`, and the command's `data`.

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "webPageId": 9,
    "webPageGuid": "5fdcf2ed-c672-46f1-b959-776142bb6a74",
    "name": "CoffeeBeveragesExplained-ouq66na8",
    "contentType": "DancingGoat.ArticlePage",
    "treePath": "/Articles/Coffee_Beverages_Explained",
    "parentWebPageId": 1,
    "languageName": "en",
    "channelName": "DancingGoatPages"
  }
}
```

## The packages

| Package | Needs the Kentico SDK? | What it is |
|---|---|---|
| `Xperience.Relay.Contracts` | No | The commands, results and envelopes every other package shares. |
| `Xperience.Relay.Core` | No | Command dispatch, the verb registry and pipeline behaviors. |
| `Xperience.Relay.Kentico` | Yes | The handlers that call Kentico, the API key store and its admin application. Deployed in the Xperience web app. |
| `Xperience.Relay.Hosting` | No | The ASP.NET Core endpoints and API key authentication. Deployed in the Xperience web app. |
| `Xperience.Relay.Client` | No | `RelayClient`, a small HTTP client for callers. |

`Contracts` and `Client` target `net8.0` and `netstandard2.0`, so callers can be .NET Framework 4.7.2 applications too. `Core`, `Hosting` and `Kentico` target `net8.0`. The Kentico packages require Xperience by Kentico 31.7.2 or later.

## Where to start

- **[Getting started](getting-started.md)**: install the packages, create an API key and make a call.
- **[How Relay works](concepts/how-it-works.md)**: the parts, and what happens to a request.
- **[Commands](commands/index.md)**: every command, with its parameters, results, examples and errors.
- **[Create, edit and publish a page](guides/page-lifecycle.md)**: a walk through the commands in order.
