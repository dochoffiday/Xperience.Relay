# Run several commands at once

`POST /api/relay/batch` runs a list of commands in one request and answers with one result per command. It saves round trips when you have several independent things to do or read.

## The request and the answer

```json
{
  "commands": [
    { "verb": "get-page-info", "parameters": { "webPageId": 9 } },
    { "verb": "get-page-info", "parameters": { "webPageId": 99999 } },
    { "verb": "get-taxonomies" }
  ]
}
```

Each entry is an ordinary [envelope](../concepts/commands-and-results.md#the-envelope). The answer holds a `results` list in the same order:

```json
{
  "results": [
    {
      "success": true,
      "message": null,
      "error": null,
      "data": { "webPageId": 9, "name": "CoffeeBeveragesExplained-ouq66na8", "treePath": "/Articles/Coffee_Beverages_Explained", "...": "..." }
    },
    {
      "success": false,
      "message": null,
      "error": "Web page 99999 was not found.",
      "data": null
    },
    {
      "success": true,
      "message": "Retrieved 6 taxonomy/taxonomies.",
      "error": null,
      "data": { "taxonomies": [ "..." ] }
    }
  ]
}
```

(Abridged.) The second command failed, and the third still ran.

## How a batch runs

- **In order.** The commands run one after another, in the order you sent them.
- **A failure doesn't stop the rest.** Every command runs and has its own result, so check each one's `success`.
- **No transaction.** Commands that succeeded are not undone when a later one fails.
- **No sharing of results.** A command can't use another's output. The envelope's `as` property is reserved for that and does nothing yet. If one step needs an ID the previous step returns (a folder, a new page), make them separate calls.
- **Authorized as a whole.** Before the first command runs, Relay checks the key's tier against every command in the batch. If any is out of reach the whole batch is refused with `403` and nothing runs:

    ```json
    {
      "success": false,
      "message": null,
      "error": "This API key has Read access, but 'move-web-page' requires Write.",
      "data": null
    }
    ```

- **Bad entries fail alone.** An unknown verb or parameters that don't fit give a failed result for that entry, and the others run.

## From .NET

```csharp
var batch = await relay.ExecuteBatchAsync(
[
    new GetPageInfoCommand { WebPageId = 9 },
    new GetTaxonomiesCommand(),
]);

var page = batch.Results[0].GetData<WebPageInfo>();
var taxonomies = batch.Results[1].GetData<GetTaxonomiesResult>();
```

`ExecuteBatchAsync` throws an `HttpRequestException` when the site answers with an HTTP error such as `401` or `403`, because there are no per-command results to return.

## When to use a batch

- **Good fit:** reading several things, or a series of changes that don't depend on each other (publishing ten pages whose IDs you know).
- **Poor fit:** a sequence where each step needs the last one's result, and anything that has to be all-or-nothing.

A batch runs inside a single HTTP request, so a long one can run into timeouts on the client or in a proxy in front of the site, and the [request size limit](../configuration.md#maxrequestbodysizebytes) applies to the whole body. Keep batches modest.
