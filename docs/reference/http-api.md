# HTTP API

The relay exposes three endpoints. Every request needs an API key. Everything is JSON.

| Endpoint | Purpose |
|---|---|
| [`POST /api/relay/commands`](#post-commands) | Run one command. |
| [`POST /api/relay/batch`](#post-batch) | Run a list of commands in order. |
| [`GET /api/relay/verbs`](#get-verbs) | List the commands the site supports. |

The `/api/relay` part is the [`BasePath`](../configuration.md#basepath), which you can change. The examples use `https://your-site.example` for the site and `YOUR_API_KEY` for a key; use HTTPS in production, since the key travels in a header.

## Authentication

Send the key in the `X-Relay-Api-Key` header (the name is [configurable](../configuration.md#apikeyheadername)):

```http
POST /api/relay/commands HTTP/1.1
Host: your-site.example
X-Relay-Api-Key: YOUR_API_KEY
Content-Type: application/json

{ "verb": "get-page-info", "parameters": { "webPageId": 9 } }
```

| Problem | Response |
|---|---|
| No key, or a key that doesn't exist (or was deleted) | `401 Unauthorized` with an empty body |
| A valid key whose [tier](../concepts/access-tiers.md) is too low for the command | `403 Forbidden` with a failed result |

A `403` body looks like this, and names both tiers:

```json
{
  "success": false,
  "message": null,
  "error": "This API key has Read access, but 'delete-web-page' requires Write.",
  "data": null
}
```

## POST /commands

Runs one command. The body is an [envelope](../concepts/commands-and-results.md#the-envelope):

```json
{
  "verb": "get-page-info",
  "parameters": { "webPageId": 9 }
}
```

The answer is a [result](../concepts/commands-and-results.md#the-result):

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

### What comes back as HTTP 200

A command that ran, whatever its outcome, is `200`. A command that couldn't be carried out has `success` set to `false` and the reason in `error`:

| Case | `error` |
|---|---|
| Unknown verb | `Unknown verb 'no-such-verb'.` |
| A parameter of the wrong type | `Could not deserialize parameters for verb 'get-page-info': The JSON value could not be converted to System.Int32. Path: $.webPageId ...` |
| `parameters` is `null` | `Parameters for verb 'get-taxonomies' resolved to null.` |
| An exception inside the command | `Unhandled exception executing verb 'create-web-page': [InvalidOperationException] ...` |

A `parameters` that is left out altogether is treated as `{}`.

### How the JSON is read

- Property names are camelCase, and matched without regard to case.
- Names that don't belong to the command are ignored.
- Enums, where there are any, are strings.
- Dates are ISO 8601 strings.

## POST /batch

Runs a list of commands in order and answers with one result for each. A failure doesn't stop the rest, and a batch is authorized as a whole before it starts.

```json
{
  "commands": [
    { "verb": "get-page-info", "parameters": { "webPageId": 9 } },
    { "verb": "get-taxonomies" }
  ]
}
```

```json
{ "results": [ { "success": true, "...": "..." }, { "success": true, "...": "..." } ] }
```

See [Run several commands at once](../guides/batches.md).

## GET /verbs

Lists the commands the deployed site supports, and the version of Xperience.Relay it runs. Any valid key can call it, whatever its tier.

```bash
curl "https://your-site.example/api/relay/verbs" -H "X-Relay-Api-Key: YOUR_API_KEY"
```

```json
{
  "relayVersion": "0.6.0.0",
  "verbs": [
    {
      "verb": "create-content-item",
      "parameters": {
        "ContentTypeName": "String",
        "DisplayName": "String",
        "LanguageName": "String",
        "WorkspaceName": "String",
        "ContentFolderId": "Nullable`1",
        "Fields": "Dictionary`2",
        "LinkedItemFields": "Dictionary`2",
        "TagFields": "Dictionary`2",
        "Assets": "List`1"
      }
    },
    {
      "verb": "create-custom-object",
      "parameters": {
        "ObjectTypeName": "String",
        "Fields": "Dictionary`2"
      }
    }
  ]
}
```

(Abridged: the real answer lists every verb.) The parameter names are the .NET property names, so they are PascalCase, and the types are .NET's short type names (`Nullable`1` for an optional value, `List`1` for a list). The listing doesn't include a command's access tier. Its use is to check that a deployed site supports a verb before you rely on it, since sites can run different versions.

## Status codes

| Status | When |
|---|---|
| `200` | The command ran, whatever the result. Check `success`. |
| `400` | The body isn't valid JSON, or isn't an envelope. |
| `401` | The API key is missing or unknown. No body. |
| `403` | The key's tier is too low. The body is a failed result. |
| `413` | The body is larger than [`MaxRequestBodySizeBytes`](../configuration.md#maxrequestbodysizebytes). |
| `500` | Something outside a command went wrong. The exception is logged on the site. |

## Notes

- **Limits.** The request body is limited to 256 MB by default, which matters for [file uploads](../guides/uploading-assets.md). Infrastructure in front of the site can have lower limits.
- **Repeating a request.** Reads are safe to repeat. So are `publish-*` and `unpublish-*`, which report "already published" or "already unpublished". Creating commands create again, and a second `delete-*` of something already deleted fails with "was not found".
- **Browsers.** Relay doesn't configure CORS. It is meant for server-to-server calls, and an API key doesn't belong in a web page.
- **Logging.** A rejected request is logged as a warning on the site, and the log entries written while a request runs carry the name of its key in a `RelayApiKey` scope, so activity can be traced to a key.
